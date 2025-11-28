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

        public ConnectedClient(TcpClient client)
        {
            Client = client;
            var stream = client.GetStream();
            Reader = new StreamReader(stream, Encoding.UTF8);
            Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = false };
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (!Client.Connected) return;
                await Writer.WriteLineAsync(message);
                await Writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gửi lỗi: {ex.Message}");
            }
        }
    }
}