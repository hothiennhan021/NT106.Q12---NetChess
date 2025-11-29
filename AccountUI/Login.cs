using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using ChessClient;
// using ChessLogic; // Bỏ comment dòng này nếu ClientManager nằm trong namespace ChessLogic

namespace AccountUI
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string tentk = textBox1.Text;
            string matkhau = textBox2.Text;

            // 1. Kiểm tra nhập liệu
            if (string.IsNullOrWhiteSpace(tentk) || tentk == "Tên Đăng Nhập")
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản!");
                return;
            }

            button1.Enabled = false;
            button1.Text = "Đang xử lý...";

            try
            {
                // -----------------------------------------------------------
                // PHẦN 1: LOGIN CHÍNH (Code của nhóm - Dùng ClientManager)
                // -----------------------------------------------------------
                // Lưu ý: Nếu IP khác 127.0.0.1 thì sửa ở đây
                await ClientManager.ConnectToServerAsync("127.0.0.1", 8888);
                
                string request = $"LOGIN|{tentk}|{matkhau}";
                await ClientManager.Instance.SendAsync(request);

                // Chờ phản hồi (Async)
                string response = await Task.Run(() => ClientManager.Instance.WaitForMessage());

                // [FIX LỖI CRASH]: Kiểm tra null trước khi cắt chuỗi
                if (string.IsNullOrEmpty(response))
                {
                    MessageBox.Show("Lỗi: Server không phản hồi hoặc mất kết nối.");
                    button1.Enabled = true;
                    button1.Text = "Đăng Nhập";
                    return;
                }

                var parts = response.Split('|');
                var command = parts[0];

                if (command == "LOGIN_SUCCESS")
                {
                    // -----------------------------------------------------------
                    // PHẦN 2: LOGIN PHỤ (Cho chức năng Bạn bè - Dùng ClientSocket)
                    // -----------------------------------------------------------
                    try 
                    {
                        // Kết nối ngầm để Server biết ID người dùng này cho chức năng Bạn bè
                        bool connected = ClientSocket.Connect("127.0.0.1", 8888);
                        if (connected)
                        {
                            // Gửi login lần 2 qua kênh này để định danh
                            ClientSocket.SendAndReceive($"LOGIN|{tentk}|{matkhau}");
                        }
                    } 
                    catch { /* Lờ đi lỗi này để không ảnh hưởng game chính */ }
                    // -----------------------------------------------------------

                    MessageBox.Show("Đăng nhập thành công!", "Thông báo");
                    this.Hide();
                    // Mở MainMenu (Code cũ của nhóm)
                    MainMenu mainmenu = new MainMenu();
                    mainmenu.ShowDialog();
                    this.Close();
                }
                else
                {
                    string msg = parts.Length > 1 ? parts[1] : "Lỗi đăng nhập";
                    MessageBox.Show(msg);
                    ClientManager.Disconnect();
                    button1.Enabled = true;
                    button1.Text = "Đăng Nhập";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
                button1.Enabled = true;
                button1.Text = "Đăng Nhập";
            }
        }

        // --- CÁC HÀM GIAO DIỆN PHỤ (Placeholder để Designer không báo lỗi thiếu hàm) ---
        // Giữ nguyên các hàm này để không bị mất sự kiện giao diện
        private void textBox1_Enter(object sender, EventArgs e) {
            if (textBox1.Text == "Tên Đăng Nhập") { textBox1.Text = ""; textBox1.ForeColor = Color.Black; }
        }
        private void textBox1_Leave(object sender, EventArgs e) {
            if (textBox1.Text == "") { textBox1.Text = "Tên Đăng Nhập"; textBox1.ForeColor = Color.Gray; }
        }
        private void textBox2_Enter(object sender, EventArgs e) {
            if (textBox2.Text == "Mật Khẩu") { textBox2.Text = ""; textBox2.ForeColor = Color.Black; textBox2.PasswordChar = '*'; }
        }
        private void textBox2_Leave(object sender, EventArgs e) {
            if (textBox2.Text == "") { textBox2.Text = "Mật Khẩu"; textBox2.ForeColor = Color.Gray; textBox2.PasswordChar = '\0'; }
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { new Signup().ShowDialog(); }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { new Recovery().ShowDialog(); }
        private void Login_Load(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void button_passwordhide_Click(object sender, EventArgs e) {
             if (textBox2.PasswordChar == '\0') { button_passwordshow.BringToFront(); textBox2.PasswordChar = '*'; }
        }
        private void button_passwordshow_Click(object sender, EventArgs e) {
             if (textBox2.PasswordChar == '*') { button_passwordhide.BringToFront(); textBox2.PasswordChar = '\0'; }
        }
    }
}