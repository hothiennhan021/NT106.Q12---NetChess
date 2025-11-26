using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChessLogic; // Đảm bảo đã using ChessLogic

namespace MyTcpServer
{
    public class GameSession
    {
        public string SessionId { get; }
        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        public ConnectedClient PlayerWhite { get; }
        public ConnectedClient PlayerBlack { get; }

        private readonly GameState _gameState;

        // --- XÓA: Không cần writer riêng ---
        // private readonly StreamWriter _writerWhite;
        // private readonly StreamWriter _writerBlack;

        // --- SỬA ĐỔI: Nhận ConnectedClient ---
        public GameSession(ConnectedClient player1, ConnectedClient player2)
        {
            SessionId = Guid.NewGuid().ToString();

            // Gán ngẫu nhiên Trắng/Đen
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

            // --- XÓA: Không tạo writer mới ---
            // _writerWhite = new StreamWriter(...)
            // _writerBlack = new StreamWriter(...)

            // Khởi tạo GameState "thật" trên server
            _gameState = new GameState(Player.White, Board.Initial());
        }

        // Bắt đầu game, gửi thông báo cho 2 client
        public async Task StartGame()
        {
            string boardString = Serialization.BoardToString(_gameState.Board);

            // --- SỬA ĐỔI: Dùng SendMessageAsync ---
            // Đây là chỗ sửa lỗi quan trọng nhất
            await PlayerWhite.SendMessageAsync($"GAME_START|WHITE|{boardString}");
            await PlayerBlack.SendMessageAsync($"GAME_START|BLACK|{boardString}");
        }

        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        // --- SỬA ĐỔI: Dùng ConnectedClient ---
        public async Task HandleMove(ConnectedClient client, string moveString)
        {
            // LOG 1: Bắt đầu
            Console.WriteLine("[GameSession LOG 1] Bắt đầu HandleMove.");

            try
            {
                // 1. Xác định người chơi
                Player player = (client == PlayerWhite) ? Player.White : Player.Black;

                // 2. Server chỉ kiểm tra 2 thứ CƠ BẢN
                if (player != _gameState.CurrentPlayer)
                {
                    await client.SendMessageAsync("ERROR|Không phải lượt của bạn.");
                    return;
                }

                // 3. Parse nước đi
                var parts = moveString.Split('|');
                Position from = new Position(int.Parse(parts[1]), int.Parse(parts[2]));
                Position to = new Position(int.Parse(parts[3]), int.Parse(parts[4]));

                // 4. Lấy quân cờ (Nhanh)
                Pieces piece = _gameState.Board[from];
                if (piece == null || piece.Color != player)
                {
                    await client.SendMessageAsync("ERROR|Lỗi (Quân cờ không tồn tại).");
                    return;
                }

                // 5. Lấy nước đi (NHANH, không gọi LegalMovesForPiece)
                Move? move = piece.GetMoves(from, _gameState.Board).FirstOrDefault(m => m.ToPos.Equals(to));

                // 6. Kiểm tra xem Client có "hack" không
                if (move == null)
                {
                    await client.SendMessageAsync("ERROR|Lỗi (Nước đi không tồn tại/Client hack).");
                    return;
                }

                // 7. SERVER TIN TƯỞNG CLIENT (BỎ QUA IsLegal)

                // LOG 2: Ngay trước khi MakeMove
                Console.WriteLine($"[GameSession LOG 2] Đã parse xong, chuẩn bị MakeMove cho {from} -> {to}");

                // 8. THỰC HIỆN NƯỚC ĐI
                _gameState.MakeMove(move);

                // LOG 3: Ngay sau khi MakeMove
                Console.WriteLine("[GameSession LOG 3] Đã MakeMove, chuẩn bị tạo chuỗi board...");

                // 9. Gửi UPDATE (Bây giờ sẽ chạy ngay lập tức)
                string boardString = Serialization.BoardToString(_gameState.Board);
                string currentPlayer = _gameState.CurrentPlayer.ToString().ToUpper();
                string updateMessage = $"UPDATE|{boardString}|{currentPlayer}";

                // LOG 4: Ngay trước khi Broadcast
                Console.WriteLine("[GameSession LOG 4] Đã tạo xong updateMessage, chuẩn bị Broadcast...");

                await Broadcast(updateMessage); //

                // LOG 5: Ngay sau khi Broadcast
                Console.WriteLine("[GameSession LOG 5] ĐÃ BROADCAST XONG.");
            }
            catch (Exception ex)
            {
                // LOG LỖI
                Console.WriteLine($"[GameSession LỖI] {ex.Message}");
                await client.SendMessageAsync($"ERROR|Lỗi Server: {ex.Message}");
            }
        }

        // --- SỬA ĐỔI: Dùng SendMessageAsync ---
        private async Task Broadcast(string message)
        {
            // Gửi cho White, bọc trong try-catch riêng
            try
            {
                await PlayerWhite.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi, nhưng KHÔNG dừng lại
                Console.WriteLine($"Lỗi khi gửi cho PlayerWhite: {ex.Message}");
            }

            // Gửi cho Black, bọc trong try-catch riêng
            try
            {
                await PlayerBlack.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi, nhưng KHÔNG dừng lại
                Console.WriteLine($"Lỗi khi gửi cho PlayerBlack: {ex.Message}");
            }
        }

        // --- SỬA ĐỔI: Thêm hàm SendToClient (vì HandleMove cần) ---
        private async Task SendToClient(ConnectedClient client, string message)
        {
            await client.SendMessageAsync(message);
        }
    }
}