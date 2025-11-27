using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MyTcpServer
{
    public static class GameManager
    {
        private static readonly ConcurrentQueue<ConnectedClient> _waitingLobby = new ConcurrentQueue<ConnectedClient>();
        private static readonly ConcurrentDictionary<ConnectedClient, GameSession> _activeGames = new ConcurrentDictionary<ConnectedClient, GameSession>();

        public static void HandleClientConnect(ConnectedClient client) { }

        public static void HandleClientDisconnect(ConnectedClient client)
        {
            if (_activeGames.TryRemove(client, out GameSession session))
            {
                // --- CHỈ GỬI THÔNG BÁO THOÁT NẾU GAME CHƯA KẾT THÚC ---
                if (session.IsGameOver() == false)
                {
                    ConnectedClient otherPlayer = (session.PlayerWhite == client) ? session.PlayerBlack : session.PlayerWhite;

                    // Gửi tin nhắn cho người còn lại (Client sẽ hiểu Resigned là game over)
                    _ = otherPlayer.SendMessageAsync("GAME_OVER_FULL|Đối thủ đã rời game!|Resigned");

                    // Xóa đối thủ khỏi activeGames
                    _activeGames.TryRemove(otherPlayer, out GameSession dummy);
                }

                Console.WriteLine($"GameSession {session.SessionId} kết thúc do người chơi thoát.");
            }
        }

        public static async Task ProcessGameCommand(ConnectedClient client, string command)
        {
            var parts = command.Split('|');
            string action = parts[0];

            if (action == "FIND_GAME")
            {
                await AddToLobby(client);
                return;
            }

            if (_activeGames.TryGetValue(client, out GameSession session))
            {
                switch (action)
                {
                    case "MOVE":
                        await session.HandleMove(client, command);
                        break;
                    case "CHAT":
                        if (parts.Length > 1) await session.BroadcastChat(client, parts[1]);
                        break;

                    // --- CHUYỂN LỆNH HỆ THỐNG CHO GAMESESSION ---
                    case "REQUEST_RESTART":
                    case "RESTART_NO":
                    case "LEAVE_GAME":
                        await session.HandleGameCommand(client, action);
                        break;
                }
            }
        }

        private static async Task AddToLobby(ConnectedClient client)
        {
            _waitingLobby.Enqueue(client);

            if (_waitingLobby.Count >= 2)
            {
                if (_waitingLobby.TryDequeue(out ConnectedClient player1) &&
                    _waitingLobby.TryDequeue(out ConnectedClient player2))
                {
                    GameSession newSession = new GameSession(player1, player2);

                    _activeGames[player1] = newSession;
                    _activeGames[player2] = newSession;

                    await newSession.StartGame();
                }
            }
            else
            {
                await client.SendMessageAsync("WAITING");
            }
        }
    }
}