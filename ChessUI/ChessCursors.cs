using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ChessUI
{
    public static class ChessCursors
    {
        // Load an toàn, nếu lỗi thì trả về null (dùng con trỏ mặc định)
        public static readonly Cursor WhiteCursor = LoadCursorSafe("pack://application:,,,/ChessUI;component/Assets/CursorW.cur");
        public static readonly Cursor BlackCursor = LoadCursorSafe("pack://application:,,,/ChessUI;component/Assets/CursorB.cur");

        private static Cursor LoadCursorSafe(string fullPackUri)
        {
            try
            {
                var streamInfo = Application.GetResourceStream(new Uri(fullPackUri, UriKind.Absolute));
                if (streamInfo != null)
                {
                    return new Cursor(streamInfo.Stream, true);
                }
                return Cursors.Arrow; // Không tìm thấy -> Dùng mũi tên thường
            }
            catch
            {
                return Cursors.Arrow; // Lỗi -> Dùng mũi tên thường
            }
        }
    }
}