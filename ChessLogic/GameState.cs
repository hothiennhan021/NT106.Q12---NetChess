using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
            // QUAN TRỌNG: Bật dòng này để khi Client nhận bàn cờ về
            // nó biết ngay là game đã kết thúc hay chưa (Checkmate/Stalemate)
            CheckForGameOver();
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            // GIỮ NGUYÊN "IsEmty" (theo code cũ của bạn) để không bị lỗi đỏ
            if (Board.IsEmty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Pieces piece = Board[pos];
            IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        public void MakeMove(Move move)
        {
            move.Execute(Board);
            CurrentPlayer = CurrentPlayer.Opponent();

            // Ở Client, chúng ta có thể tắt dòng này nếu muốn Server quyết định.
            // Nhưng để an toàn cho logic hiển thị, bật nó lên cũng không sao.
            CheckForGameOver();
        }

        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Pieces piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });
            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        private void CheckForGameOver()
        {
            // Kiểm tra nếu không còn nước đi hợp lệ
            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsIncheck(CurrentPlayer))
                {
                    // Bị chiếu hết
                    Result = Result.Win(CurrentPlayer.Opponent());
                }
                else
                {
                    // Hết nước đi (Hòa)
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        // Hàm hỗ trợ lấy nước đi nhanh cho Client vẽ HighLight
        public IEnumerable<Move> MovesForPiece(Position pos)
        {
            // GIỮ NGUYÊN "IsEmty"
            if (Board.IsEmty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }
            Pieces piece = Board[pos];
            return piece.GetMoves(pos, Board);
        }
    }
}