using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyTcpServer
{
    public static class GameManager
    {
        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        private static readonly ConcurrentQueue<ConnectedClient> _waitingLobby = new ConcurrentQueue<ConnectedClient>();
        private static readonly ConcurrentDictionary<ConnectedClient, GameSession> _activeGames = new ConcurrentDictionary<ConnectedClient, GameSession>();

        // --- HÀM MỚI ---
        public static void HandleClientConnect(ConnectedClient client)
        {
            // (Có thể thêm logic nếu cần)
            Console.WriteLine("GameManager đã ghi nhận client mới.");
        }

        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        public static void HandleClientDisconnect(ConnectedClient client)
        {
            if (_activeGames.TryRemove(client, out GameSession? session))
            {
                // (Thêm logic thông báo cho đối thủ)
            }
            // (Thêm logic xóa client khỏi lobby nếu đang chờ)
            Console.WriteLine("Client đã ngắt kết nối và được dọn dẹp khỏi GameManager.");
        }

        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        public static async Task ProcessGameCommand(ConnectedClient client, string command)
        {
            var parts = command.Split('|');
            string action = parts[0];

            switch (action)
            {
                case "FIND_GAME":
                    await AddToLobby(client);
                    break;

                case "MOVE":
                    if (_activeGames.TryGetValue(client, out GameSession? session))
                    {
                        await session.HandleMove(client, command);
                    }
                    break;
            }
        }

        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        private static async Task AddToLobby(ConnectedClient client)
        {
            _waitingLobby.Enqueue(client);

            if (_waitingLobby.Count >= 2)
            {
                if (_waitingLobby.TryDequeue(out ConnectedClient?player1) &&
                    _waitingLobby.TryDequeue(out ConnectedClient? player2))
                {
                    // Truyền ConnectedClient
                    GameSession newSession = new GameSession(player1, player2);

                    _activeGames[player1] = newSession;
                    _activeGames[player2] = newSession;

                    Console.WriteLine($"Đã ghép cặp! Bắt đầu GameSession: {newSession.SessionId}");

                    await newSession.StartGame();
                }
            }
            else
            {
                // --- SỬA ĐỔI: Dùng Writer có sẵn ---
                // Đây là chỗ sửa lỗi quan trọng nhất
                await client.SendMessageAsync("WAITING");
                Console.WriteLine("Client đã vào hàng đợi.");
            }
        }
    }
}