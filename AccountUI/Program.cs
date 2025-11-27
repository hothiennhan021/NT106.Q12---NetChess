using System;
using System.Windows.Forms;
using ChessClient;

namespace AccountUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1. Chạy màn hình Đăng nhập
            Login loginForm = new Login();
            Application.Run(loginForm);

            // 2. Sau khi Login đóng, kiểm tra xem đã kết nối chưa
            if (ClientManager.Instance.IsConnected)
            {
                // Nếu đã kết nối (tức là đăng nhập thành công), mở MainMenu
                MainMenu mainMenu = new MainMenu();
                Application.Run(mainMenu);
            }

            // Nếu tắt Login mà chưa kết nối -> Chương trình tự kết thúc sạch sẽ.
        }
    }
}