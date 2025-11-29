namespace ChessClient.Models
{
    public class ChessApiResponse
    {
        // Lời bình (VD: "Mate in 2")
        public string text { get; set; }

        // Nước đi tốt nhất (VD: "e2e4")
        public string move { get; set; }

        // Điểm lợi thế (Cho phép null vì khi Chiếu hết sẽ không có điểm này)
        // Dấu ? nghĩa là có thể null
        public int? centipawns { get; set; }

        // Số nước chiếu hết (Đổi sang int? vì API trả về số, không phải chữ)
        public int? mate { get; set; }
    }

    // Class hỗ trợ lưu Cache (giữ nguyên)
    public class MoveAnalysis
    {
        public int MoveIndex { get; set; }
        public string Fen { get; set; }
        public ChessApiResponse Result { get; set; }
    }
}