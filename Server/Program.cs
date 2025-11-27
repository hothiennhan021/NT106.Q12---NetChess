using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ChessData;
using MyTcpServer;

namespace MyTcpServer
{
    class Program
    {
        private static IConfiguration _config;
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
                        return await _userRepo.LoginUserAsync(parts[1], parts[2]);
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

                default:
                    return "ERROR|Lệnh không xác định.";
            }
        }
    }
}