using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MyTcpServer; // --- THÊM MỚI ---

namespace MyTcpServer
{
    class Program
    {
        private static IConfiguration _config;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _config = builder.Build();

            int port = 8888;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server TCP đã khởi động trên cổng {port}...");
            Console.WriteLine("Đang chờ kết nối từ client...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client đã kết nối!");
                _ = HandleClientAsync(client);
            }
        }

        // --- SỬA ĐỔI HandleClientAsync ---
        static async Task HandleClientAsync(TcpClient client)
        {
            // 1. Tạo một đối tượng client bao bọc
            ConnectedClient connectedClient = new ConnectedClient(client);

            try
            {
                // 2. Đăng ký client với GameManager
                GameManager.HandleClientConnect(connectedClient);

                while (true)
                {
                    // 3. Dùng Reader từ connectedClient
                    string requestMessage = await connectedClient.Reader.ReadLineAsync();
                    if (requestMessage == null) break; // Client ngắt kết nối

                    Console.WriteLine($"Đã nhận: {requestMessage}");

                    // 4. Truyền connectedClient vào ProcessRequest
                    string responseMessage = await ProcessRequest(connectedClient, requestMessage);

                    if (responseMessage != null)
                    {
                        // 5. Dùng hàm SendMessageAsync
                        await connectedClient.SendMessageAsync(responseMessage);
                        Console.WriteLine($"Đã gửi: {responseMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi client: " + ex.Message);
            }
            finally
            {
                // 6. Thông báo GameManager dùng connectedClient
                GameManager.HandleClientDisconnect(connectedClient);
                connectedClient.Reader.Close();
                connectedClient.Writer.Close();
                connectedClient.Client.Close();
            }
        }

        // --- SỬA ĐỔI HÀM NÀY ---
        static async Task<string> ProcessRequest(ConnectedClient client, string requestMessage)
        {
            string[] parts = requestMessage.Split('|');
            string command = parts[0];

            switch (command)
            {
                case "REGISTER":
                    // (Logic REGISTER của bạn giữ nguyên)
                    if (parts.Length == 6)
                    {
                        return await HandleRegister(parts[1], parts[2], parts[3], parts[4], parts[5]);
                    }
                    return "ERROR|Giao thức REGISTER không đúng.";

                case "LOGIN":
                    // (Logic LOGIN của bạn giữ nguyên)
                    if (parts.Length == 3)
                    {
                        return await HandleLogin(parts[1], parts[2]);
                    }
                    return "ERROR|Giao thức LOGIN không đúng.";

                // --- THÊM MỚI: Chuyển các lệnh game cho GameManager ---
                case "FIND_GAME":
                case "MOVE":
                    // 7. Truyền connectedClient
                    await GameManager.ProcessGameCommand(client, requestMessage);
                    return null; // Trả về null vì GameManager sẽ tự xử lý

                default:
                    return "ERROR|Lệnh không xác định.";
            }
        }


        // --- CÁC HÀM LOGIN/REGISTER CỦA BẠN (giữ nguyên) ---
        static async Task<string> HandleRegister(string username, string rawPassword, string email, string fullName, string birthday)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);
            await using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string checkUserSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                await using (SqlCommand checkCmd = new SqlCommand(checkUserSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Username", username);
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    int userCount = (int)await checkCmd.ExecuteScalarAsync();
                    if (userCount > 0)
                    {
                        return "ERROR|Tên đăng nhập hoặc Email đã tồn tại.";
                    }
                }
                string registerSql = "INSERT INTO Users (Username, PasswordHash, Email, FullName, Birthday) " +
                                     "VALUES (@Username, @PasswordHash, @Email, @FullName, @Birthday)";
                await using (SqlCommand cmd = new SqlCommand(registerSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Birthday", Convert.ToDateTime(birthday));
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return "REGISTER_SUCCESS|Đăng ký thành công!";
        }

        static async Task<string> HandleLogin(string username, string rawPassword)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");
            string storedHash = "";
            string fullName = "";
            string email = "";
            string birthday = "";
            await using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT PasswordHash, FullName, Email, Birthday FROM Users WHERE Username = @Username";
                await using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    await using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            storedHash = reader["PasswordHash"].ToString();
                            fullName = reader["FullName"]?.ToString() ?? "";
                            email = reader["Email"]?.ToString() ?? "";
                            DateTime? dbDate = reader["Birthday"] as DateTime?;
                            birthday = dbDate.HasValue ? dbDate.Value.ToString("yyyy-MM-dd") : "";
                        }
                        else
                        {
                            return "ERROR|Tên đăng nhập hoặc mật khẩu không đúng.";
                        }
                    }
                }
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(rawPassword, storedHash);
            if (isPasswordValid)
            {
                return $"LOGIN_SUCCESS|{fullName}|{email}|{birthday}";
            }
            else
            {
                return "ERROR|Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
    }
}