using ChessLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessUI
{
    public static class Images
    {
        // Bạn cứ để tên file ở đây, code bên dưới sẽ tự tìm đúng file thật
        private static readonly Dictionary<PieceType, ImageSource> whiteSources = new()
        {
            {PieceType.Pawn, LoadImage("PawnW.png") },
            {PieceType.Bishop, LoadImage("BishopW.png") },
            {PieceType.Knight, LoadImage("KnightW.png") },
            {PieceType.Rook, LoadImage("RookW.png") },
            {PieceType.Queen, LoadImage("QueenW.png") },
            {PieceType.King, LoadImage("KingW.png") },
        };

        private static readonly Dictionary<PieceType, ImageSource> blackSources = new()
        {
            {PieceType.Pawn, LoadImage("PawnB.png") },
            {PieceType.Bishop, LoadImage("BishopB.png") },
            {PieceType.Knight, LoadImage("KnightB.png") },
            {PieceType.Rook, LoadImage("RookB.png") },
            {PieceType.Queen, LoadImage("QueenB.png") },
            {PieceType.King, LoadImage("KingB.png") },
        };

        private static ImageSource LoadImage(string fileName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // --- KỸ THUẬT TÌM KIẾM THÔNG MINH ---
                // Lấy tất cả tài nguyên, tìm cái nào có đuôi trùng với fileName (không phân biệt hoa thường)
                string resourcePath = assembly.GetManifestResourceNames()
                    .FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(resourcePath)) return null; // Không thấy thì thôi, không sập

                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream == null) return null;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }

        public static ImageSource GetImage(Player color, PieceType type)
        {
            return color == Player.White ? whiteSources[type] : blackSources[type];
        }

        public static ImageSource GetImage(Pieces piece)
        {
            if (piece == null) return null;
            return GetImage(piece.Color, piece.Type);
        }
    }
}