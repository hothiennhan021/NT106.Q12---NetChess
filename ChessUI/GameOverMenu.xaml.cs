using ChessLogic;
using System;
using System.Windows;
using System.Windows.Controls;


namespace ChessUI
{
    public partial class GameOverMenu : UserControl
    {
        // Sự kiện để báo cho MainWindow biết người dùng chọn gì
        public event Action<Option> OptionSelected;

        // Constructor nhận GameState để hiển thị thông tin thắng thua
        public GameOverMenu(GameState gameState)
        {
            InitializeComponent();

            Result result = gameState.Result;

            // Kiểm tra null để tránh crash nếu game kết thúc bất ngờ
            if (result == null)
            {
                WinnerText.Text = "GAME OVER";
                ReasonText.Text = "Kết thúc (Đối thủ thoát/Đầu hàng)";
            }
            else
            {
                WinnerText.Text = GetWinnerText(result.Winner);
                ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
            }
        }

        // Hàm hỗ trợ lấy text
        private static string GetWinnerText(Player winner)
        {
            return winner switch
            {
                Player.White => "WHITE WINS",
                Player.Black => "BLACK WINS",
                _ => "IT'S A DRAW"
            };
        }

        private static string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => "WHITE",
                Player.Black => "BLACK",
                _ => ""
            };
        }

        private static string GetReasonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => $"STALEMATE - {PlayerString(currentPlayer)} CAN'T MOVE",
                EndReason.Checkmate => $"CHECKMATE - {PlayerString(currentPlayer)} CAN'T MOVE",
                EndReason.FiftyMoveRule => "FIFTY-MOVE RULE",
                EndReason.InsufficientMaterial => "INSUFFICIENT MATERIAL",
                EndReason.ThreefoldRepetition => "THREEFOLD REPETITION",
                _ => ""
            };
        }
        public void DisableRestartButton()
        {
            if (BtnRestart != null)
            {
                BtnRestart.IsEnabled = false;     //khong cho bấm
                BtnRestart.Content = "Đã thoát";    // Đổi chữ
                BtnRestart.Opacity = 0.5;           // Làm mờ đi
            }
        }

        // Xử lý sự kiện Click
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}