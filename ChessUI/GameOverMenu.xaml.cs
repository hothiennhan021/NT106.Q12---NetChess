using System;
using System.Windows;
using System.Windows.Controls;
using ChessLogic; // Đảm bảo đã Reference project ChessLogic

namespace ChessUI
{
    public partial class GameOverMenu : UserControl
    {
        // Sự kiện để báo ra ngoài cho MainWindow biết
        public event Action<Option> OptionSelected;

        public GameOverMenu()
        {
            InitializeComponent();
        }

        // Hàm hiển thị thông tin (MainWindow sẽ gọi hàm này)
        public void ShowGameOver(string winner, string reason)
        {
            // Cập nhật Text người thắng
            if (winner == "Draw")
            {
                WinnerText.Text = "HÒA CỜ (DRAW)";
            }
            else
            {
                string winnerName = (winner == "White") ? "TRẮNG" : "ĐEN";
                WinnerText.Text = $"{winnerName} THẮNG";
            }

            // Cập nhật Lý do
            ReasonText.Text = $"Lý do: {reason}";

            // Mở khóa nút Chơi lại (đề phòng bị disable từ ván trước)
            if (BtnRestart != null)
            {
                BtnRestart.IsEnabled = true;
                BtnRestart.Content = "CHƠI LẠI";
                BtnRestart.Opacity = 1.0;
            }
        }

        // Hàm khóa nút (được gọi khi đã bấm Chơi lại để tránh spam)
        public void DisableRestartButton()
        {
            if (BtnRestart != null)
            {
                BtnRestart.IsEnabled = false;
                BtnRestart.Content = "Đang chờ...";
                BtnRestart.Opacity = 0.5;
            }
        }

        // --- CÁC SỰ KIỆN CLICK (Phải khớp với XAML) ---

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            // Báo ra ngoài là người dùng chọn Phân tích
            OptionSelected?.Invoke(Option.Analyze);
        }
    }
}