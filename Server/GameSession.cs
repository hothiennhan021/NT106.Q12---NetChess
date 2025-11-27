using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChessLogic;

namespace MyTcpServer
{
    public class GameSession
    {
        public string SessionId { get; }
        public ConnectedClient PlayerWhite { get; }
        public ConnectedClient PlayerBlack { get; }

        private GameState _gameState;
        private readonly ChessTimer _gameTimer; // Dùng ChessTimer (từ ChessLogic)
        private readonly ChatRoom _chatRoom;

        private bool _whiteWantsRematch = false;
        private bool _blackWantsRematch = false;

        public GameSession(ConnectedClient player1, ConnectedClient player2)
        {
            SessionId = Guid.NewGuid().ToString();

            if (new Random().Next(2) == 0) { PlayerWhite = player1; PlayerBlack = player2; }
            else { PlayerWhite = player2; PlayerBlack = player1; }

            _gameState = new GameState(Player.White, Board.Initial());
            _gameTimer = new ChessTimer(10); // 10 phút
            _gameTimer.TimeExpired += HandleTimeExpired;
            _chatRoom = new ChatRoom(PlayerWhite, PlayerBlack);
        }

        // --- GETTER PUBLIC ĐỂ GAMEMANAGER KIỂM TRA TRẠNG THÁI ---
        public bool IsGameOver() => _gameState.IsGameOver();

        public async Task StartGame()
        {
            _gameTimer.Start(Player.White);
            string board = Serialization.BoardToString(_gameState.Board);
            int wTime = _gameTimer.WhiteRemaining;
            int bTime = _gameTimer.BlackRemaining;

            await PlayerWhite.SendMessageAsync($"GAME_START|WHITE|{board}|{wTime}|{bTime}");
            await PlayerBlack.SendMessageAsync($"GAME_START|BLACK|{board}|{wTime}|{bTime}");
        }

        public async Task HandleMove(ConnectedClient client, string moveString)
        {
            try
            {
                Player player = (client == PlayerWhite) ? Player.White : Player.Black;
                if (player != _gameState.CurrentPlayer) return;

                // 1. Parse
                var parts = moveString.Split('|');
                if (parts.Length != 5) return;
                int r1 = int.Parse(parts[1]); int c1 = int.Parse(parts[2]);
                int r2 = int.Parse(parts[3]); int c2 = int.Parse(parts[4]);

                Position from = new Position(r1, c1);
                Position to = new Position(r2, c2);

                // 2. Check nước đi hợp lệ
                Pieces piece = _gameState.Board[from];
                if (piece == null || piece.Color != player) return;

                IEnumerable<Move> moves = piece.GetMoves(from, _gameState.Board);
                Move move = moves.FirstOrDefault(m => m.ToPos.Equals(to));

                if (move == null) return;

                // 3. Thực hiện và chuyển lượt
                _gameState.MakeMove(move);
                _gameTimer.SwitchTurn();

                // 4. Gửi Update
                string boardStr = Serialization.BoardToString(_gameState.Board);
                string curPlayer = _gameState.CurrentPlayer.ToString().ToUpper();
                await Broadcast($"UPDATE|{boardStr}|{curPlayer}|{_gameTimer.WhiteRemaining}|{_gameTimer.BlackRemaining}");

                // 5. Check Win
                if (_gameState.IsGameOver())
                {
                    _gameTimer.Stop();
                    Player winner = _gameState.Result.Winner;
                    string wStr = (winner == Player.White) ? "White" : (winner == Player.Black ? "Black" : "Draw");
                    string reason = _gameState.Result.Reason.ToString();
                    await Broadcast($"GAME_OVER_FULL|{wStr}|{reason}");
                }
            }
            catch (Exception ex) { Console.WriteLine($"Move Error: {ex.Message}"); }
        }

        // --- XỬ LÝ LỆNH HỆ THỐNG (Rematch, Leave) ---
        public async Task HandleGameCommand(ConnectedClient client, string command)
        {
            if (command == "REQUEST_RESTART")
            {
                if (client == PlayerWhite) _whiteWantsRematch = true; else _blackWantsRematch = true;
                ConnectedClient opp = (client == PlayerWhite) ? PlayerBlack : PlayerWhite;

                if (_whiteWantsRematch && _blackWantsRematch) await RestartGame();
                else await opp.SendMessageAsync("ASK_RESTART");
            }
            else if (command == "RESTART_NO")
            {
                _whiteWantsRematch = false; _blackWantsRematch = false;
                ConnectedClient opp = (client == PlayerWhite) ? PlayerBlack : PlayerWhite;
                await opp.SendMessageAsync("RESTART_DENIED");
            }
            else if (command == "LEAVE_GAME")
            {
                ConnectedClient opp = (client == PlayerWhite) ? PlayerBlack : PlayerWhite;
                // Chỉ cần báo cho đối thủ biết, GameManager sẽ lo việc dọn dẹp sau
                await opp.SendMessageAsync("OPPONENT_LEFT");
            }
        }

        private async Task RestartGame()
        {
            _gameState = new GameState(Player.White, Board.Initial());
            _gameTimer.Stop(); _gameTimer.Sync(600, 600); // Reset Timer
            _whiteWantsRematch = false; _blackWantsRematch = false;
            await StartGame();
        }

        private void HandleTimeExpired(Player loser)
        {
            string winner = (loser == Player.White) ? "Black" : "White";
            _ = Broadcast($"GAME_OVER_FULL|{winner}|TimeOut");
        }

        public async Task BroadcastChat(ConnectedClient sender, string msg) { await _chatRoom.SendMessage(sender, msg); }

        private async Task Broadcast(string msg)
        {
            try { await PlayerWhite.SendMessageAsync(msg); } catch { }
            try { await PlayerBlack.SendMessageAsync(msg); } catch { }
        }
    }
}