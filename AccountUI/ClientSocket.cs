using System;
using System.Net.Sockets;
using System.Text;

namespace AccountUI
{
    public static class ClientSocket
    {
        // Biến static để lưu giữ kết nối duy nhất
        private static TcpClient _client;
        private static NetworkStream _stream;

        // Cấu hình Server
        private const string SERVER_IP = "127.0.0.1";
        private const int PORT = 8888;

        // Hàm kết nối thông minh
        public static bool Connect(string ipAddress = SERVER_IP, int port = PORT)
        {
            try
            {
                // [QUAN TRỌNG] Nếu đã có kết nối và đang sống -> KHÔNG TẠO MỚI
                if (_client != null && _client.Connected)
                {
                    return true;
                }

                // Nếu chưa có hoặc bị đứt -> Mới tạo lại
                _client = new TcpClient();
                var result = _client.BeginConnect(ipAddress, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                if (!success) return false;

                _client.EndConnect(result);
                _stream = _client.GetStream();
                return true;
            }
            catch
            {
                _client = null;
                _stream = null;
                return false;
            }
        }

        public static string SendAndReceive(string message)
        {
            // Tự động kết nối lại nếu lỡ rớt mạng (nhưng sẽ mất login state nếu server không lưu session)
            if (_client == null || !_client.Connected)
            {
                if (!Connect()) return "ERROR|Mất kết nối Server";
            }

            try
            {
                // Gửi tin nhắn (Thêm xuống dòng \n)
                byte[] dataToSend = Encoding.UTF8.GetBytes(message + "\n");
                _stream.Write(dataToSend, 0, dataToSend.Length);
                _stream.Flush();

                // Nhận phản hồi
                byte[] buffer = new byte[4096];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0) return "ERROR|Server đóng kết nối";

                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                return response;
            }
            catch (Exception ex)
            {
                Disconnect(); // Lỗi thì reset
                return "ERROR|Lỗi mạng: " + ex.Message;
            }
        }

        public static void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                _client = null;
                _stream = null;
            }
            catch { }
        }
    }
}