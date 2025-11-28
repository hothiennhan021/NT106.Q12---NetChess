using ChessLogic;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using ChessClient;

namespace AccountUI
{
    public partial class Login : Form
    {
        // Cấu hình IP Server (Sửa lại IP AWS của bạn nếu cần)
        private const string SERVER_IP = "20.2.251.78";
        private const int SERVER_PORT = 8888;

        public Login()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string tentk = textBox1.Text;
            string matkhau = textBox2.Text;

            button1.Enabled = false;
            button1.Text = "Đang xử lý...";

            try
            {
                // Kết nối
                await ClientManager.ConnectToServerAsync(SERVER_IP, SERVER_PORT);

                // Gửi đăng nhập
                string request = $"LOGIN|{tentk}|{matkhau}";
                await ClientManager.Instance.SendAsync(request);

                // Chờ phản hồi
                string response = await Task.Run(() => ClientManager.Instance.WaitForMessage());

                if (response == null) throw new Exception("Không nhận được phản hồi.");

                var parts = response.Split('|');
                var command = parts[0];

                if (command == "LOGIN_SUCCESS")
                {
                    MessageBox.Show("Đăng nhập thành công!", "Thông báo");

                    // --- QUAN TRỌNG: CHỈ ĐÓNG FORM, KHÔNG MỞ MENU ---
                    this.Close();
                }
                else
                {
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Lỗi", "Thất bại");
                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
                ResetUI();
            }
        }

        private void ResetUI()
        {
            button1.Enabled = true;
            button1.Text = "Đăng Nhập";
            ClientManager.Disconnect();
        }

        // --- CÁC HÀM UI KHÁC GIỮ NGUYÊN ---
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { Signup s = new Signup(); s.ShowDialog(); }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { Recovery r = new Recovery(); r.ShowDialog(); }
        private void textBox1_Enter(object sender, EventArgs e) { if (textBox1.Text == "Tên Đăng Nhập") { textBox1.Text = ""; textBox1.ForeColor = Color.DarkSlateBlue; } }
        private void textBox1_Leave(object sender, EventArgs e) { if (textBox1.Text == "") { textBox1.Text = "Tên Đăng Nhập"; textBox1.ForeColor = Color.Gray; } }
        private void textBox2_Enter(object sender, EventArgs e) { if (textBox2.Text == "Mật Khẩu") { textBox2.Text = ""; textBox2.ForeColor = Color.DarkSlateBlue; textBox2.PasswordChar = '*'; } }
        private void textBox2_Leave(object sender, EventArgs e) { if (textBox2.Text == "") { textBox2.Text = "Mật Khẩu"; textBox2.ForeColor = Color.Gray; textBox2.PasswordChar = '\0'; } }
        private void button_passwordhide_Click(object sender, EventArgs e) { if (textBox2.PasswordChar == '\0') { button_passwordshow.BringToFront(); textBox2.PasswordChar = '*'; } }
        private void button_passwordshow_Click(object sender, EventArgs e) { if (textBox2.PasswordChar == '*') { button_passwordhide.BringToFront(); textBox2.PasswordChar = '\0'; } }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void Login_Load(object sender, EventArgs e) { }
    }
}