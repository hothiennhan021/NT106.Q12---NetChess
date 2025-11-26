using ChessLogic;
using ChessUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace AccountUI
{
    public partial class MainMenu : Form
    {
        // --- ĐÃ XÓA ---
        // Không cần luồng lắng nghe (_listenerTask) ở đây nữa.

        public MainMenu()
        {
            InitializeComponent();

            // --- ĐÃ XÓA ---
            // Không bắt đầu lắng nghe ở đây.
            // StartServerListener(); 
        }

        // --- ĐÃ XÓA ---
        // Toàn bộ hàm "StartServerListener()" đã bị xóa
        // vì nó gây ra lỗi "cạnh tranh luồng".

        // --- HÀM XỬ LÝ TIN NHẮN (Giữ nguyên) ---
        private void HandleServerMessage(string message)
        {
            var parts = message.Split('|');
            var command = parts[0];

            if (command == "GAME_START")
            {
                // 1. Ẩn Menu đi thay vì đóng hẳn
                this.Hide();

                // 2. Mở Game
                LaunchWpfGameWindow(message);
            }
            else if (command == "WAITING")
            {
                button1.Text = "Đang chờ đối thủ...";
            }
        }

        // --- HÀM KHỞI CHẠY WPF (Giữ nguyên) ---
        // Trong MainMenu.cs

        // Trong file MainMenu.cs

        // Trong file MainMenu.cs

        private void LaunchWpfGameWindow(string gameStartMessage)
        {
            // 1. Ẩn Menu hiện tại
            this.Hide();

            // 2. Tạo luồng mới chạy Game WPF
            Thread wpfThread = new Thread(() =>
            {
                try
                {
                    // Định nghĩa hành động khi thoát Game (để truyền vào Constructor MainWindow)
                    Action onGameExit = () =>
                    {
                        // Hành động này được gọi từ bên WPF (MainWindow.xaml.cs)
                        // Chúng ta để trống ở đây cũng được, vì ta sẽ xử lý chính ở khối finally bên dưới
                        // để đảm bảo an toàn tuyệt đối.
                    };

                    // Khởi tạo cửa sổ Game
                    ChessUI.MainWindow gameWindow = new ChessUI.MainWindow(gameStartMessage, onGameExit);

                    // Chặn luồng này lại cho đến khi Game đóng
                    gameWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi chạy Game: " + ex.Message);
                }
                finally
                {
                    // [QUAN TRỌNG NHẤT]
                    // Khối này LUÔN LUÔN chạy khi cửa sổ Game đóng lại (dù lỗi hay không)

                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        // Dùng BeginInvoke để đẩy lệnh Show() về luồng chính của Form MainMenu
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.Show();
                            this.BringToFront();

                            if (this.WindowState == FormWindowState.Minimized)
                                this.WindowState = FormWindowState.Normal;

                            // Reset trạng thái nút bấm
                            button1.Text = "Tìm trận nhanh";
                            button1.Enabled = true;
                            button3.Enabled = true;
                        });
                    }
                    else
                    {
                        // Nếu Menu đã bị hủy thì tắt luôn app
                        Application.Exit();
                    }
                }
            });

            wpfThread.SetApartmentState(ApartmentState.STA);
            wpfThread.IsBackground = true;
            wpfThread.Start();
        }
        // --- THAY THẾ TOÀN BỘ HÀM NÀY ---
        // Hàm "button1_Click" cũ đã bị thay thế bằng hàm "async" mới
        private async void button1_Click(object sender, EventArgs e)
        {
            // 1. Cập nhật UI
            button1.Text = "Đang tìm trận...";
            button1.Enabled = false;
            button3.Enabled = false;

            try
            {
                // 2. Gửi lệnh FIND_GAME
                await ClientManager.Instance.SendAsync("FIND_GAME");

                // 3. CHỜ tin nhắn đầu tiên (phải là WAITING hoặc GAME_START)
                // (Chạy trên luồng nền để không treo UI)
                string response = await Task.Run(() => ClientManager.Instance.WaitForMessage());

                // Xử lý tin WAITING (hoặc GAME_START nếu ghép ngay)
                // (Lưu ý: HandleServerMessage đang chạy trên luồng UI vì .Invoke trong code gốc)
                // Cần đảm bảo nó được gọi trên luồng UI
                this.Invoke((MethodInvoker)delegate
                {
                    HandleServerMessage(response);
                });


                // 4. Nếu server báo "WAITING", chúng ta cần chờ tin thứ hai (là GAME_START)
                if (response.StartsWith("WAITING"))
                {
                    // Chờ tin GAME_START
                    response = await Task.Run(() => ClientManager.Instance.WaitForMessage());

                    this.Invoke((MethodInvoker)delegate
                    {
                        HandleServerMessage(response);
                    });
                }

                // Khi HandleServerMessage nhận GAME_START, nó sẽ tự đóng Form này
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm trận: {ex.Message}");
                // Reset lại nút
                button1.Text = "Tìm trận nhanh";
                button1.Enabled = true;
                button3.Enabled = true;
            }
        }

        // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---
        private void button3_Click(object sender, EventArgs e)
        {
            // (Chức năng khác)
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {

        }
    }
}