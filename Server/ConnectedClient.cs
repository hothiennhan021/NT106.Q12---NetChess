using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyTcpServer
{
    // Class này để gói gọn TcpClient lại, giúp Server quản lý dễ hơn
    public class ConnectedClient
    {
        public TcpClient Client { get; }
        public StreamReader Reader { get; }
        public StreamWriter Writer { get; }

        // [QUAN TRỌNG] Biến này để lưu ID người dùng sau khi đăng nhập thành công
        // Mặc định là 0 (chưa đăng nhập)
        public int UserId { get; set; } = 0;

        public ConnectedClient(TcpClient client)
        {
            Client = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream, Encoding.UTF8);
            Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = false };
        }

        // Hàm gửi tin nhắn an toàn, không gây crash server nếu client rớt mạng
        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (Client.Connected)
                {
                    await Writer.WriteLineAsync(message);
                    await Writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi tin tới User {UserId}: {ex.Message}");
            }
        }

        // Hàm ngắt kết nối 
        public void Close()
        {
            try
            {
                Reader.Close();
                Writer.Close();
                Client.Close();
            }
            catch { /* Bỏ qua lỗi khi đóng */ }
        }
    }
}