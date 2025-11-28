using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ChessData;
using MyTcpServer;
using Microsoft.Data.SqlClient;

namespace MyTcpServer
{
    class Program
    {
        private static IConfiguration _config;
        private static FriendRepository _friendRepo;
        private static UserRepository _userRepo;

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
                        // 1. Gọi hàm đăng nhập
                        string result = await _userRepo.LoginUserAsync(parts[1], parts[2]);

                        // 2. Nếu đăng nhập thành công -> Bắt buộc phải lấy UserId
                        if (result.StartsWith("LOGIN_SUCCESS"))
                        {
                            string username = parts[1];

                            // Gọi hàm lấy ID (đảm bảo hàm GetUserIdByUsername đã có ở cuối file)
                            int uid = GetUserIdByUsername(username);

                            if (uid > 0)
                            {
                                client.UserId = uid; // Gán ID chuẩn
                                Console.WriteLine($"[DEBUG] User {username} đã đăng nhập với ID: {uid}");
                            }
                            else
                            {
                                // Nếu không tìm thấy ID (lỗi lạ), thử tìm lại lần nữa hoặc báo lỗi
                                Console.WriteLine($"[LỖI] Đăng nhập thành công nhưng không tìm thấy UserId cho: {username}");
                            }
                        }

                        return result;
                    }
                    return "ERROR|Format LOGIN sai.";

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