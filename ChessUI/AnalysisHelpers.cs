using System;
using System.Windows.Media;

namespace ChessUI
{
    // Chỉ còn 4 loại
    public enum MoveQuality
    {
        Brilliant,  // !!
        Best,       // ★ (Gộp cả Good/Excellent vào đây)
        Mistake,    // ? (Gộp Inaccuracy vào đây)
        Blunder     // ??
    }

    public static class MoveClassifier
    {
        private const int MATE_SCORE = 30000;

        public static MoveQuality Classify(int? prevCp, int? prevMate, int? currCp, int? currMate, bool isWhiteTurn)
        {
            int scoreBefore = ConvertToScore(prevCp, prevMate);
            int scoreAfter = ConvertToScore(currCp, currMate);

            // Tính độ mất điểm
            int loss = isWhiteTurn ? (scoreBefore - scoreAfter) : (scoreAfter - scoreBefore);

            // LOGIC PHÂN LOẠI 4 MỨC:

            // 1. Brilliant: Nếu nước đi làm tăng lợi thế bất ngờ (Loss âm)
            if (loss < 0) return MoveQuality.Brilliant;

            // 2. Best: Mất ít hơn 50cp (Gộp Best, Excellent, Good)
            if (loss <= 50) return MoveQuality.Best;

            // 3. Mistake: Mất từ 50 đến 300cp (Gộp Inaccuracy, Mistake)
            if (loss <= 300) return MoveQuality.Mistake;

            // 4. Blunder: Mất trên 300cp
            return MoveQuality.Blunder;
        }

        private static int ConvertToScore(int? cp, int? mate)
        {
            if (cp.HasValue) return cp.Value;
            if (mate.HasValue)
            {
                int sign = Math.Sign(mate.Value);
                return (sign * MATE_SCORE) - (mate.Value * 100);
            }
            return 0;
        }

        public static string GetColorHex(MoveQuality quality)
        {
            switch (quality)
            {
                case MoveQuality.Brilliant: return "#1AB5B8"; // Xanh ngọc
                case MoveQuality.Best: return "#6BC235"; // Xanh lá
                case MoveQuality.Mistake: return "#FFA333"; // Cam
                case MoveQuality.Blunder: return "#FA412D"; // Đỏ
                default: return "#FFFFFF";
            }
        }

        // Đường dẫn chính xác tới 4 file ảnh của bạn
        public static string GetIconPath(MoveQuality quality)
        {
            // Lưu ý: Đảm bảo bạn đã copy 4 ảnh vào thư mục Assets/Icons/
            string basePath = "/ChessUI;component/Assets/Icons/";
            switch (quality)
            {
                case MoveQuality.Brilliant: return basePath + "brilliant.png";
                case MoveQuality.Best: return basePath + "best.png";
                case MoveQuality.Mistake: return basePath + "mistake.png";
                case MoveQuality.Blunder: return basePath + "blunder.png";
                default: return null;
            }
        }
    }
}