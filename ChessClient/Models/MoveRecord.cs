namespace ChessUI.Models
{
    public class MoveRecord
    {
        public int TurnNumber { get; set; }

        // TRẮNG
        public string WhiteMove { get; set; } // Text: "e4"
        public string WhiteEval { get; set; } // Text: "+0.5"
        public string WhiteColor { get; set; }
        public int WhiteIndex { get; set; }
        public string WhiteIcon { get; set; } // [MỚI] Đường dẫn ảnh icon

        // ĐEN
        public string BlackMove { get; set; }
        public string BlackEval { get; set; }
        public string BlackColor { get; set; }
        public int BlackIndex { get; set; }
        public string BlackIcon { get; set; } // [MỚI] Đường dẫn ảnh icon
    }
}