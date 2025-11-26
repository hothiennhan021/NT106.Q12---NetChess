//using ChessUI;
using System.Threading.Tasks;

namespace ChessLogic
{
    // Lớp này sẽ "sống" xuyên suốt ứng dụng
    // Nó giữ một thể hiện (instance) duy nhất của NetworkClient
    public static class ClientManager
    {
        // Thể hiện duy nhất của NetworkClient
        private static NetworkClient _instance;

        // Cung cấp một cách an toàn để truy cập vào thể hiện
        public static NetworkClient Instance
        {
            get
            {
                // Nếu _instance là null (chưa có)
                // HOẶC _instance đã bị ngắt kết nối (từ lần test trước)
                if (_instance == null || !_instance.IsConnected)
                {
                    // Tạo một instance SẠCH
                    _instance = new NetworkClient();
                }
                return _instance;
            }
        }

        // Hàm hỗ trợ để kết nối một lần
        // (AccountUI sẽ gọi hàm này sau khi đăng nhập)
        public static async Task ConnectToServerAsync(string ip, int port)
        {
            if (Instance.IsConnected) return;

            await Instance.ConnectAsync(ip, port);
        }

        // Hàm hỗ trợ để ngắt kết nối
        public static void Disconnect()
        {
            Instance.CloseConnection();
            _instance = null;
        }
    }
}