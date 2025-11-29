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
        private readonly List<string> _moveHistory = new List<string>();

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

        // Trong Server/GameSession.cs

        public async Task HandleMove(ConnectedClient client, string moveString)
        {
            try
            {
                Player player = (client == PlayerWhite) ? Player.White : Player.Black;
                if (player != _gameState.CurrentPlayer) return;

                // 1. Phân tích chuỗi (Parse)
                var parts = moveString.Split('|');
                // MOVE | r1 | c1 | r2 | c2 | [promotionType]
                // parts[0] là "MOVE", nên r1 bắt đầu từ parts[1]

                if (parts.Length < 5) return;

                int r1 = int.Parse(parts[1]);
                int c1 = int.Parse(parts[2]);
                int r2 = int.Parse(parts[3]);
                int c2 = int.Parse(parts[4]);

                Position from = new Position(r1, c1);
                Position to = new Position(r2, c2);

                // 2. Tìm nước đi phù hợp trong danh sách nước đi hợp lệ
                Pieces piece = _gameState.Board[from];
                if (piece == null || piece.Color != player) return;

                IEnumerable<Move> moves = _gameState.MovesForPiece(from);

                Move move = null;

                // --- XỬ LÝ PHONG CẤP (PROMOTION) ---
                if (parts.Length == 6) // Có thêm tham số loại quân (VD: 4 = Queen)
                {
                    int typeId = int.Parse(parts[5]);
                    PieceType promoType = (PieceType)typeId;

                    // Tìm nước đi PawnPromotion khớp với vị trí và loại quân
                    move = moves.OfType<PawnPromotion>()
                                .FirstOrDefault(m => m.ToPos == to && m.newType == promoType); // Lưu ý: Cần public property 'newType' trong PawnPromotion
                }
                else
                {
                    // Nước đi thường hoặc Nhập thành (Castling cũng tự động nhận diện qua ToPos)
                    move = moves.FirstOrDefault(m => m.ToPos == to && !(m is PawnPromotion));
                }

                if (move == null || !move.IsLegal(_gameState.Board))
                {
                    Console.WriteLine("Nước đi không hợp lệ hoặc không tìm thấy trên Server.");
                    return;
                }

                // 3. Thực hiện và chuyển lượt
                _gameState.MakeMove(move);
                _gameTimer.SwitchTurn();

                string uciString = UciConverter.MoveToUci(move);
                _moveHistory.Add(uciString);

                // 4. Gửi Update cho cả 2 người chơi
                string boardStr = Serialization.BoardToString(_gameState.Board);
                string curPlayer = _gameState.CurrentPlayer.ToString().ToUpper();

                // Gửi thêm thông tin thời gian
                await Broadcast($"UPDATE|{boardStr}|{curPlayer}|{_gameTimer.WhiteRemaining}|{_gameTimer.BlackRemaining}");

                // 5. Kiểm tra kết quả trận đấu
                if (_gameState.IsGameOver())
                {
                    _gameTimer.Stop();
                    Player winner = _gameState.Result.Winner;
                    string wStr = (winner == Player.White) ? "White" : (winner == Player.Black ? "Black" : "Draw");
                    string reason = _gameState.Result.Reason.ToString();
                    await Broadcast($"GAME_OVER_FULL|{wStr}|{reason}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Move Error: {ex.Message} \n {ex.StackTrace}");
            }
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
        public async Task HandleAnalysisRequest(ConnectedClient client)
        {
            // Kiểm tra xem người yêu cầu có phải là người chơi trong phòng không
            if (client != PlayerWhite && client != PlayerBlack) return;

            // Ghép danh sách nước đi thành chuỗi: "e2e4 e7e5 g1f3..."
            string data = string.Join(" ", _moveHistory);

            // Gửi về Client
            await client.SendMessageAsync($"ANALYSIS_DATA|{data}");
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