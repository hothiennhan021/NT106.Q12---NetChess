using System.Collections.Generic;

namespace ChessClient.Models
{
    // Class ánh xạ JSON trả về từ Lichess API
    public class LichessAnalysisResponse
    {
        public string fen { get; set; }
        public List<MoveEvaluation> pvs { get; set; }
    }

    public class MoveEvaluation
    {
        public string moves { get; set; } // Chuỗi các nước đi gợi ý
        public int cp { get; set; }       // Điểm lợi thế (Centipawns)
        public int mate { get; set; }     // Số nước chiếu hết (nếu có)
    }
}