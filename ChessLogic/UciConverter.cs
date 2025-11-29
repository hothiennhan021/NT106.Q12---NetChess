using System.Text;

namespace ChessLogic
{
    public static class UciConverter
    {
        // Hàm chuyển đổi 1 nước đi (Move) sang chuỗi UCI (vd: "e2e4")
        public static string MoveToUci(Move move)
        {
            if (move == null) return "";

            // 1. Lấy tọa độ từ và đến
            string from = PositionToUci(move.FromPos);
            string to = PositionToUci(move.ToPos);

            // 2. Xử lý phong cấp (Promotion)
            string promotion = "";
            if (move.Type == MoveType.PawnPromotion && move is PawnPromotion promo)
            {
                promotion = promo.newType switch
                {
                    PieceType.Queen => "q",
                    PieceType.Rook => "r",
                    PieceType.Bishop => "b",
                    PieceType.Knight => "n",
                    _ => ""
                };
            }

            return $"{from}{to}{promotion}";
        }

        // Hàm phụ: Chuyển đổi Position(Row, Col) sang "e2", "a1"...
        private static string PositionToUci(Position pos)
        {
            // Lưu ý: Cần kiểm tra logic Row/Col trong project của bạn.
            // Giả sử: Col 0='a', Col 7='h'.
            // Giả sử: Row 0=Rank 8 (trên cùng), Row 7=Rank 1 (dưới cùng).
            // Nếu Row 0 của bạn là Rank 1, hãy đổi công thức bên dưới.

            char file = (char)('a' + pos.Column);
            int rank = 8 - pos.Row;

            return $"{file}{rank}";
        }
    }
}