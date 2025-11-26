using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChessLogic; // Đảm bảo project này đã có ChessTimer

namespace MyTcpServer
{
    public class GameSession
    {
        public string SessionId { get; }
        public ConnectedClient PlayerWhite { get; }
        public ConnectedClient PlayerBlack { get; }

        // --- TÍNH NĂNG 1: CỜ HIỆU CHƠI LẠI ---
        public bool WhiteWantsRestart { get; set; } = false;
        public bool BlackWantsRestart { get; set; } = false;

        private GameState _gameState;

        // --- TÍNH NĂNG 2: ĐỒNG HỒ & CHAT ---
        private ChessTimer _gameTimer;

        // (Nếu bạn chưa có class ChatRoom thì có thể bỏ qua dòng này và hàm BroadcastChat, 
        // nhưng mình khuyến khích nên tạo class ChatRoom đơn giản để quản lý)
        // private readonly ChatRoom _chatRoom; 

        public GameSession(ConnectedClient player1, ConnectedClient player2)
        {
            SessionId = Guid.NewGuid().ToString();

            // 1. Random màu
            if (new Random().Next(2) == 0)
            {
                PlayerWhite = player1;
                PlayerBlack = player2;
            }
            else
            {
                PlayerWhite = player2;
                PlayerBlack = player1;
            }

            // 2. Khởi tạo GameState & Timer
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _gameState = new GameState(Player.White, Board.Initial());

            // Nếu timer cũ đang chạy thì dừng lại
            if (_gameTimer != null) _gameTimer.Stop();

            _gameTimer = new ChessTimer(10); // 10 phút
            _gameTimer.TimeExpired += HandleTimeExpired; // Sự kiện khi hết giờ
        }

        // --- XỬ LÝ GAME ---

        public async Task StartGame()
        {
            // Bắt đầu tính giờ cho Trắng
            _gameTimer.Start(Player.White);

            string boardString = Serialization.BoardToString(_gameState.Board);
            int wTime = _gameTimer.WhiteRemaining;
            int bTime = _gameTimer.BlackRemaining;

            // Gửi tin nhắn GỒM CẢ THỜI GIAN (để khớp với Client mới)
            await PlayerWhite.SendMessageAsync($"GAME_START|WHITE|{boardString}|{wTime}|{bTime}");
            await PlayerBlack.SendMessageAsync($"GAME_START|BLACK|{boardString}|{wTime}|{bTime}");
        }

        public async Task ResetGame()
        {
            WhiteWantsRestart = false;
            BlackWantsRestart = false;
            InitializeComponents();
            await StartGame();
        }

        public async Task HandleMove(ConnectedClient client, string moveString)
        {
            try
            {
                Player player = (client == PlayerWhite) ? Player.White : Player.Black;

                // 1. Kiểm tra lượt (Server Authority)
                if (player != _gameState.CurrentPlayer)
                {
                    await client.SendMessageAsync("ERROR|Chưa đến lượt của bạn!");
                    return;
                }

                // 2. Parse tọa độ
                var parts = moveString.Split('|');
                Position from = new Position(int.Parse(parts[1]), int.Parse(parts[2]));
                Position to = new Position(int.Parse(parts[3]), int.Parse(parts[4]));

                // 3. Lấy quân cờ
                Pieces piece = _gameState.Board[from];
                if (piece == null || piece.Color != player) return;

                // 4. Tìm nước đi hợp lệ (Server check kỹ)
                // Lưu ý: Hàm này gọi GetMoves -> cần đảm bảo ChessLogic chuẩn
                Move move = piece.GetMoves(from, _gameState.Board).FirstOrDefault(m => m.ToPos.Equals(to));

                if (move == null)
                {
                    await client.SendMessageAsync("ERROR|Nước đi không hợp lệ (Server reject).");
                    return;
                }

                // 5. Thực hiện nước đi
                _gameState.MakeMove(move);

                // 6. ĐẢO LƯỢT ĐỒNG HỒ
                _gameTimer.SwitchTurn();

                // 7. Gửi UPDATE KÈM THỜI GIAN
                string boardString = Serialization.BoardToString(_gameState.Board);
                string currentPlayerStr = _gameState.CurrentPlayer.ToString().ToUpper();
                int wTime = _gameTimer.WhiteRemaining;
                int bTime = _gameTimer.BlackRemaining;

                // Format: UPDATE | BOARD | PLAYER | WHITE_TIME | BLACK_TIME
                string updateMsg = $"UPDATE|{boardString}|{currentPlayerStr}|{wTime}|{bTime}";
                await Broadcast(updateMsg);

                // 8. Kiểm tra Hết cờ (Checkmate/Draw)
                if (_gameState.IsGameOver())
                {
                    _gameTimer.Stop(); // Dừng đồng hồ
                    string winnerMsg = "Game Over";

                    if (_gameState.Result.Winner == Player.White) winnerMsg = "TRẮNG thắng!";
                    else if (_gameState.Result.Winner == Player.Black) winnerMsg = "ĐEN thắng!";
                    else winnerMsg = "HÒA cờ!";

                    await Broadcast($"GAME_OVER|{winnerMsg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi GameSession] {ex.Message}");
            }
        }

        // --- XỬ LÝ SỰ KIỆN TIMER ---
        private void HandleTimeExpired(Player loser)
        {
            string winner = (loser == Player.White) ? "Đen" : "Trắng";
            _ = Broadcast($"GAME_OVER|{winner} thắng do đối thủ hết giờ!");
        }

        // --- XỬ LÝ CHAT ---
        public async Task BroadcastChat(ConnectedClient sender, string messageContent)
        {
            // Gửi lại tin nhắn cho CẢ HAI (hoặc chỉ đối thủ, tùy logic client)
            // Ở đây mình gửi cho đối thủ để họ hiện lên
            ConnectedClient opponent = (sender == PlayerWhite) ? PlayerBlack : PlayerWhite;
            await opponent.SendMessageAsync($"CHAT|{messageContent}");
        }

        // --- HÀM HỖ TRỢ ---
        private async Task Broadcast(string message)
        {
            try { await PlayerWhite.SendMessageAsync(message); } catch { }
            try { await PlayerBlack.SendMessageAsync(message); } catch { }
        }
    }
}