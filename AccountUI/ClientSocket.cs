using System;
using System.Net.Sockets;
using System.Text;

namespace AccountUI
{
    public static class ClientSocket
    {
        private static TcpClient? client;
        private static NetworkStream? stream;

        // --- CẤU HÌNH IP SERVER AWS ---
        // Đây là IP Public của máy EC2 bạn vừa tạo
        private const string SERVER_IP = "54.66.192.227";
        private const int PORT = 8888;

        // Hàm kết nối (được gọi tự động)
        public static bool Connect()
        {
            try
            {
                // Nếu client cũ đã bị hủy hoặc ngắt kết nối, tạo mới
                if (client == null || !client.Connected)
                {
                    client = new TcpClient();
                    // Kết nối đến Server AWS với thời gian chờ (timeout) ngắn để không treo máy lâu
                    var result = client.BeginConnect(SERVER_IP, PORT, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3)); // Chờ 3 giây

                    if (!success)
                    {
                        client = null;
                        return false;
                    }

                    client.EndConnect(result);
                    stream = client.GetStream();
                }
                return true;
            }
            catch (Exception)
            {
                client = null;
                stream = null;
                return false;
            }
        }

        public static string SendAndReceive(string message)
        {
            // 1. TỰ ĐỘNG KẾT NỐI: Kiểm tra xem đã kết nối chưa, nếu chưa thì kết nối lại
            if (client == null || !client.Connected || stream == null)
            {
                if (!Connect())
                {
                    return "ERROR|Không thể kết nối đến Server AWS (Kiểm tra mạng).";
                }
            }

            try
            {
                // 2. Gửi dữ liệu
                byte[] dataToSend = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(dataToSend, 0, dataToSend.Length);
                stream.Flush(); // Đẩy dữ liệu đi ngay lập tức

                // 3. Nhận phản hồi
                byte[] buffer = new byte[4096]; // Tăng buffer lên chút cho an toàn
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0) return "ERROR|Server đã đóng kết nối.";

                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                return response;
            }
            catch (Exception ex)
            {
                // Nếu lỗi, reset kết nối để lần sau tự kết nối lại
                Disconnect();
                return "ERROR|Mất kết nối: " + ex.Message;
            }
        }

        public static void Disconnect()
        {
            try
            {
                stream?.Close();
                client?.Close();
                client = null;
                stream = null;
            }
            catch { }
        }
    }
}