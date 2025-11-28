using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ChessData;
using MyTcpServer;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;

namespace MyTcpServer
{
    class Program
    {
        private static IConfiguration _config;
        private static FriendRepository _friendRepo;
        private static UserRepository _userRepo;

        // Danh bạ cho luồng Game
        public static System.Collections.Concurrent.ConcurrentDictionary<int, ConnectedClient> GameUsers = new System.Collections.Concurrent.ConcurrentDictionary<int, ConnectedClient>();

        // Danh bạ cho luồng Bạn Bè
        public static System.Collections.Concurrent.ConcurrentDictionary<int, ConnectedClient> FriendUsers = new System.Collections.Concurrent.ConcurrentDictionary<int, ConnectedClient>();
        public static ConcurrentDictionary<int, ConnectedClient> OnlineUsers = new ConcurrentDictionary<int, ConnectedClient>();
        static async Task Main(string[] args)
        {
            // 1. Load cấu hình
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _config = builder.Build();

            // 2. Kết nối DB
            string connString = _config.GetConnectionString("DefaultConnection");
            try
            {
                _userRepo = new UserRepository(connString);
                _friendRepo = new FriendRepository(connString);
                Console.WriteLine("Database: OK.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return;
            }

            // 3. Mở Server
            int port = 8888;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            ConnectedClient connectedClient = new ConnectedClient(client);
            try
            {
                GameManager.HandleClientConnect(connectedClient);

                while (true)
                {
                    string requestMessage = await connectedClient.Reader.ReadLineAsync();
                    if (requestMessage == null) break;

                    Console.WriteLine($"[RECV] {requestMessage}");

                    string response = await ProcessRequest(connectedClient, requestMessage);
                    if (response != null)
                    {
                        await connectedClient.SendMessageAsync(response);
                        Console.WriteLine($"[SENT] {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client Error: {ex.Message}");
            }
            finally
            {
                if (connectedClient.UserId > 0)
                {
                    try
                    {
                        using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                        {
                            conn.Open();
                            new SqlCommand($"UPDATE Users SET IsOnline = 0 WHERE UserId = {connectedClient.UserId}", conn).ExecuteNonQuery();
                        }
                    }
                    catch { }
                    OnlineUsers.TryRemove(connectedClient.UserId, out _);
                    Console.WriteLine($"[INFO] User {connectedClient.Username} (ID: {connectedClient.UserId}) đã thoát.");
                }
                GameManager.HandleClientDisconnect(connectedClient);
                try { connectedClient.Client.Close(); } catch { }
            }
        }

        static async Task<string> ProcessRequest(ConnectedClient client, string requestMessage)
        {
            string[] parts = requestMessage.Split('|');
            string command = parts[0];

            switch (command)
            {
                case "REGISTER":
                    if (parts.Length == 6)
                        return await _userRepo.RegisterUserAsync(parts[1], parts[2], parts[3], parts[4], parts[5]);
                    return "ERROR|Format REGISTER sai.";

                case "LOGIN":
                    if (parts.Length == 3)
                    {
                        string result = await _userRepo.LoginUserAsync(parts[1], parts[2]);

                        if (result.StartsWith("LOGIN_SUCCESS"))
                        {
                            int uid = GetUserIdByUsername(parts[1]);
                            client.UserId = uid;
                            client.Username = parts[1]; // Lưu tên để hiển thị khi mời
                                                        // Cập nhật DB thành Online
                            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                            {
                                conn.Open();
                                new SqlCommand($"UPDATE Users SET IsOnline = 1 WHERE UserId = {uid}", conn).ExecuteNonQuery();
                            }

                            // [MỚI] Đăng nhập thành công -> Thêm vào danh bạ Online
                            if (uid > 0)
                            {
                                OnlineUsers.AddOrUpdate(uid, client, (k, v) => client);
                                Console.WriteLine($"[ONLINE] User {parts[1]} đã vào danh sách online.");
                            }
                        }
                        return result;
                    }
                    return "ERROR|Format LOGIN sai.";

                // --- CHÈN THÊM VÀO PROGRAM.CS ---

                case "LOGIN_FRIEND":
                    // Xử lý đăng nhập cho kênh bạn bè (Kết nối ngầm)
                    if (parts.Length == 3)
                    {
                        // 1. Gọi hàm đăng nhập để kiểm tra user/pass
                        string res = await _userRepo.LoginUserAsync(parts[1], parts[2]);

                        if (res.StartsWith("LOGIN_SUCCESS"))
                        {
                            // 2. Lấy ID từ Username
                            int uid = GetUserIdByUsername(parts[1]);

                            // [CỰC KỲ QUAN TRỌNG] Gán thông tin vào chính kết nối này
                            // Nếu thiếu dòng này, các lệnh FRIEND_INVITE sau đó sẽ báo lỗi "Chưa đăng nhập"
                            client.UserId = uid;
                            client.Username = parts[1];

                            // 3. Lưu vào danh sách FRIEND (để nhận thư mời)
                            FriendUsers.AddOrUpdate(uid, client, (k, v) => client);

                            // 4. Lưu LUÔN vào danh sách GAME (để ghép trận)
                            // Mục đích: Nếu Server không tìm thấy kết nối Game chính (do lag/lỗi), 
                            // nó sẽ dùng tạm kết nối này để bắt đầu trận đấu luôn -> Chắc chắn vào được game.
                            GameUsers.AddOrUpdate(uid, client, (k, v) => client);

                            Console.WriteLine($"[FRIEND ONLINE] User {parts[1]} (ID: {uid}) đã đăng nhập kênh Bạn Bè.");
                            return "LOGIN_FRIEND_OK";
                        }
                    }
                    return "ERROR|Sai tài khoản hoặc mật khẩu";

                // --- CÁC LỆNH GAME ---
                case "FIND_GAME":
                case "MOVE":
                case "CHAT":
                case "REQUEST_RESTART":
                case "RESTART_NO":
                case "LEAVE_GAME":
                    await GameManager.ProcessGameCommand(client, requestMessage);
                    return null; // GameManager tự gửi phản hồi
                                 // --- BẮT ĐẦU ĐOẠN CODE BẠN BÈ ---
                case "FRIEND_SEARCH":
                    if (client.UserId == 0) return "ERROR|Bạn chưa đăng nhập!";
                    return $"FRIEND_RESULT|{_friendRepo.SendFriendRequest(client.UserId, parts[1])}";

                case "FRIEND_GET_LIST":
                    if (client.UserId == 0) return "ERROR|Bạn chưa đăng nhập!";
                    var list = _friendRepo.GetListFriends(client.UserId);
                    return $"FRIEND_LIST|{string.Join(";", list)}";

                case "FRIEND_GET_REQUESTS":
                    if (client.UserId == 0) return "ERROR|Bạn chưa đăng nhập!";
                    var reqs = _friendRepo.GetFriendRequests(client.UserId);
                    return $"FRIEND_REQUESTS|{string.Join(";", reqs)}";

                case "FRIEND_ACCEPT":
                    if (client.UserId == 0) return "ERROR|Bạn chưa đăng nhập!";
                    if (int.TryParse(parts[1], out int reqId))
                    {
                        _friendRepo.AcceptFriend(reqId);
                        return "FRIEND_ACCEPT_OK";
                    }
                    return "ERROR|Sai ID lời mời";

                    // các lệnh chức năng bạn bè

                case "FRIEND_INVITE":
                    if (client.UserId == 0) return "ERROR|Bạn chưa đăng nhập!";

                    string targetName = parts[1];
                    int targetId = GetUserIdByUsername(targetName);

                    if (targetId > 0 && OnlineUsers.TryGetValue(targetId, out ConnectedClient friendClient))
                    {
                        friendClient.Mailbox.Add($"INVITE|{client.Username}");

                        // [DEBUG] In ra xem mình đang bỏ thư vào túi của ai
                        Console.WriteLine($"[DEBUG] Đã bỏ thư vào Client {friendClient.GetHashCode()} của User {targetName}");

                        return "SUCCESS|Đã gửi lời mời thách đấu!";
                    }
                    return "ERROR|Người chơi không online.";

                case "CHECK_MAIL":
                    // [DEBUG] In ra xem ai đang check mail
                    // (Bỏ comment dòng dưới nếu muốn xem liên tục, nhưng nó sẽ spam log rất nhiều)
                    // Console.WriteLine($"[DEBUG] Client {client.GetHashCode()} đang check mail. Số thư: {client.Mailbox.Count}");

                    if (client.Mailbox.Count > 0)
                    {
                        string msg = client.Mailbox[0];
                        client.Mailbox.RemoveAt(0);

                        // [DEBUG] In ra khi lấy được thư
                        Console.WriteLine($"[DEBUG] Client {client.GetHashCode()} ĐÃ LẤY ĐƯỢC THƯ: {msg}");

                        return msg;
                    }
                    return "EMPTY";

                case "FRIEND_RESPONSE":
                    string inviterName = parts[1];
                    string status = parts[2];

                    if (status == "ACCEPTED")
                    {
                        int inviterId = GetUserIdByUsername(inviterName); // ID người mời (A)

                        // --- SỬA ĐOẠN NÀY ---

                        // 1. Tìm Người Mời (A)
                        ConnectedClient clientA = null;
                        if (GameUsers.ContainsKey(inviterId)) clientA = GameUsers[inviterId];
                        else if (FriendUsers.ContainsKey(inviterId)) clientA = FriendUsers[inviterId];

                        // 2. Người Nhận (B) chính là người đang gửi lệnh này!
                        // Không cần tìm đâu xa, lấy luôn client hiện tại.
                        ConnectedClient clientB = client;

                        // 3. Ghép trận
                        if (clientA != null && clientB != null)
                        {
                            Console.WriteLine($"[MATCH] Ghép: {clientA.Username} (A) vs {clientB.Username} (B)");

                            // Gửi tin nhắn cho A biết
                            clientA.Mailbox.Add($"RESPONSE|{client.Username}|ACCEPTED");

                            // BẮT ĐẦU GAME
                            await GameManager.StartFriendMatch(clientA, clientB);

                            return "SUCCESS";
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Không tìm thấy người mời (A). ID={inviterId}");
                            return "ERROR|Người mời đã offline.";
                        }
                    }
                    return "ERROR";

                // --- KẾT THÚC ĐOẠN CODE BẠN BÈ ---

                default:
                    return "ERROR|Lệnh không xác định.";
            }
        }
        
        private static int GetUserIdByUsername(string username)
        {
            try
            {
                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Username = @u", conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    object res = cmd.ExecuteScalar();
                    return res != null ? (int)res : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Lỗi lấy ID]: " + ex.Message);
                return 0;
            }
        }
    }
}