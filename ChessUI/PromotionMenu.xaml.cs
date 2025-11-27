using ChessLogic; // Đảm bảo bạn đã có namespace này chứa enum PieceType
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for PromotionMenu.xaml
    /// </summary>
    public partial class PromotionMenu : UserControl
    {
        // Sự kiện gửi ra ngoài để báo cho Bàn cờ biết người dùng chọn gì
        public event Action<PieceType> PieceSelected;

        public PromotionMenu(Player player)
        {
            InitializeComponent();

            QueenImg.Source = Images.GetImage(player, PieceType.Queen);
            BishopImg.Source = Images.GetImage(player, PieceType.Bishop);
            RookImg.Source = Images.GetImage(player, PieceType.Rook);
            KnightImg.Source = Images.GetImage(player, PieceType.Knight);
        }

        // 1. Xử lý khi click vào Hậu (Queen)
        private void QueenImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen);
        }

        // 2. Xử lý khi click vào Tượng (Bishop)
        private void BishopImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop);
        }

        // 3. Xử lý khi click vào Xe (Rook)
        private void RookImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook);
        }

        // 4. Xử lý khi click vào Mã (Knight)
        private void KnightImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight);
        }
    }
}