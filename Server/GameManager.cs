using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyTcpServer
{
    public static class GameManager
    {
        private static readonly List<ConnectedClient> _waitingLobby = new List<ConnectedClient>();
        private static readonly object _lobbyLock = new object();
        private static readonly ConcurrentDictionary<ConnectedClient, GameSession> _activeGames = new ConcurrentDictionary<ConnectedClient, GameSession>();

        public static void HandleClientConnect(ConnectedClient client) { }

        public static void HandleClientDisconnect(ConnectedClient client)
        {
            // Xóa khỏi hàng đợi nếu đang chờ
            lock (_lobbyLock) { _waitingLobby.Remove(client); }

            // Xử lý nếu đang trong game
            if (_activeGames.TryRemove(client, out GameSession session))
            {
                if (!session.IsGameOver())
                {
                    // Tìm người còn lại
                    var other = (session.PlayerWhite == client) ? session.PlayerBlack : session.PlayerWhite;

                    // Gửi tin nhắn thắng cuộc cho người còn lại
                    _ = other.SendMessageAsync("GAME_OVER_FULL|Đối thủ thoát|Resigned");

                    // Xóa người còn lại khỏi danh sách active luôn
                    _activeGames.TryRemove(other, out _);
                }
            }
        }

        public static async Task ProcessGameCommand(ConnectedClient client, string command)
        {
            var parts = command.Split('|');

            // Xử lý tìm trận
            if (parts[0] == "FIND_GAME")
            {
                await AddToLobby(client);
                return;
            }

            // Xử lý các lệnh trong game
            if (_activeGames.TryGetValue(client, out GameSession session))
            {
                if (parts[0] == "MOVE")
                    await session.HandleMove(client, command);
                else if (parts[0] == "CHAT" && parts.Length > 1)
                    await session.BroadcastChat(client, parts[1]);
                else if (parts[0] == "REQUEST_ANALYSIS")
                {
                    await session.HandleAnalysisRequest(client);
                }
                else
                {
                    await session.HandleGameCommand(client, parts[0]);

                    // Nếu là lệnh thoát, dọn dẹp ngay lập tức
                    if (parts[0] == "LEAVE_GAME")
                    {
                        _activeGames.TryRemove(session.PlayerWhite, out _);
                        _activeGames.TryRemove(session.PlayerBlack, out _);
                    }
                }
            }
        }

        private static async Task AddToLobby(ConnectedClient client)
        {
            GameSession sessionToStart = null;

            // BƯỚC 1: Thao tác với hàng đợi (Cần Lock)
            lock (_lobbyLock)
            {
                _waitingLobby.RemoveAll(c => !c.Client.Connected); // Dọn dẹp kết nối chết

                if (!_waitingLobby.Contains(client))
                    _waitingLobby.Add(client);

                // Nếu đủ 2 người thì ghép cặp
                if (_waitingLobby.Count >= 2)
                {
                    var p1 = _waitingLobby[0];
                    var p2 = _waitingLobby[1];
                    _waitingLobby.RemoveRange(0, 2); // Xóa 2 người khỏi hàng đợi

                    // Tạo session mới
                    sessionToStart = new GameSession(p1, p2);
                    _activeGames[p1] = sessionToStart;
                    _activeGames[p2] = sessionToStart;
                }
            }
            // KẾT THÚC LOCK TẠI ĐÂY

            // BƯỚC 2: Gửi tin nhắn mạng (Async - Không được nằm trong Lock)
            if (sessionToStart != null)
            {
                await sessionToStart.StartGame();
            }
            else
            {
                await client.SendMessageAsync("WAITING");
            }
        }
    }
}