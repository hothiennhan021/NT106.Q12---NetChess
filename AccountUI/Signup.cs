using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AccountUI
{
    public partial class Signup : Form
    {
        public Signup()
        {
            InitializeComponent();
        }

        private void Signup_Load(object sender, EventArgs e)
        {
        }

        // --- SỰ KIỆN NÚT ĐĂNG KÝ ---
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu
            string tentk = txtUsername.Text;
            string email = txtEmail.Text;
            string matkhau = txtPassword.Text;
            string xnmatkhau = txtConfirmPass.Text;
            string fullName = txtFullName.Text;
            string birthday = dtpBirthday.Value.ToString("yyyy-MM-dd");

            // 2. Validation
            if (!Regex.IsMatch(tentk, @"^[A-Za-z0-9]{6,24}$") || tentk == "Tên Đăng Nhập")
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản dài 6-24 ký tự (không dấu)...", "Chú Ý");
                return;
            }
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9_.]{3,20}@gmail.com(.vn|)$") || email == "Email")
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng email @gmail.com!");
                return;
            }
            if (fullName.Trim() == "" || fullName == "Họ và Tên")
            {
                MessageBox.Show("Vui lòng nhập họ và tên!");
                return;
            }
            if (!Regex.IsMatch(matkhau, @"^[A-Za-z0-9]{6,24}$") || matkhau == "Mật Khẩu")
            {
                MessageBox.Show("Vui lòng nhập MẬT KHẨU dài 6-24 ký tự...", "Lỗi");
                return;
            }
            if (xnmatkhau != matkhau)
            {
                MessageBox.Show("Vui lòng xác nhận mật khẩu chính xác!");
                return;
            }

            // 3. Gửi Server
            try
            {
                if (!ClientSocket.Connect("127.0.0.1", 8888))
                {
                    MessageBox.Show("Không thể kết nối Server!", "Lỗi");
                    return;
                }

                string request = $"REGISTER|{tentk}|{matkhau}|{email}|{fullName}|{birthday}";
                string response = ClientSocket.SendAndReceive(request);
                string[] parts = response.Split('|');

                if (parts[0] == "REGISTER_SUCCESS")
                {
                    MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show(parts.Length > 1 ? parts[1] : "Lỗi server", "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        // --- SỰ KIỆN NÚT QUAY LẠI ---
        private void BtnBack_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Quay Lại màn hình chính?", "Chú Ý", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // --- CÁC SỰ KIỆN UI (Enter/Leave) ---

        private void TxtUsername_Enter(object sender, EventArgs e)
        {
            if (txtUsername.Text == "Tên Đăng Nhập")
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = Color.DarkSlateBlue;
            }
        }
        private void TxtUsername_Leave(object sender, EventArgs e)
        {
            if (txtUsername.Text == "")
            {
                txtUsername.Text = "Tên Đăng Nhập";
                txtUsername.ForeColor = Color.Gray;
            }
        }

        private void TxtEmail_Enter(object sender, EventArgs e)
        {
            if (txtEmail.Text == "Email")
            {
                txtEmail.Text = "";
                txtEmail.ForeColor = Color.DarkSlateBlue;
            }
        }
        private void TxtEmail_Leave(object sender, EventArgs e)
        {
            if (txtEmail.Text == "")
            {
                txtEmail.Text = "Email";
                txtEmail.ForeColor = Color.Gray;
            }
        }

        private void TxtFullName_Enter(object sender, EventArgs e)
        {
            if (txtFullName.Text == "Họ và Tên")
            {
                txtFullName.Text = "";
                txtFullName.ForeColor = Color.DarkSlateBlue;
            }
        }
        private void TxtFullName_Leave(object sender, EventArgs e)
        {
            if (txtFullName.Text == "")
            {
                txtFullName.Text = "Họ và Tên";
                txtFullName.ForeColor = Color.Gray;
            }
        }

        private void TxtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == "Mật Khẩu")
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.DarkSlateBlue;
                txtPassword.PasswordChar = '*';
            }
        }
        private void TxtPassword_Leave(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                txtPassword.Text = "Mật Khẩu";
                txtPassword.ForeColor = Color.Gray;
                txtPassword.PasswordChar = '\0';
            }
        }

        private void TxtConfirmPass_Enter(object sender, EventArgs e)
        {
            if (txtConfirmPass.Text == "Xác Nhận Mật Khẩu")
            {
                txtConfirmPass.Text = "";
                txtConfirmPass.ForeColor = Color.DarkSlateBlue;
                txtConfirmPass.PasswordChar = '*';
            }
        }
        private void TxtConfirmPass_Leave(object sender, EventArgs e)
        {
            if (txtConfirmPass.Text == "")
            {
                txtConfirmPass.Text = "Xác Nhận Mật Khẩu";
                txtConfirmPass.ForeColor = Color.Gray;
                txtConfirmPass.PasswordChar = '\0';
            }
        }

        // --- CÁC SỰ KIỆN ẨN/HIỆN PASSWORD ---

        private void BtnHidePass_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '\0')
            {
                btnShowPass.BringToFront();
                txtPassword.PasswordChar = '*';
            }
        }
        private void BtnShowPass_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '*')
            {
                btnHidePass.BringToFront();
                txtPassword.PasswordChar = '\0';
            }
        }

        private void BtnHideConfirm_Click(object sender, EventArgs e)
        {
            if (txtConfirmPass.PasswordChar == '\0')
            {
                btnShowConfirm.BringToFront();
                txtConfirmPass.PasswordChar = '*';
            }
        }
        private void BtnShowConfirm_Click(object sender, EventArgs e)
        {
            if (txtConfirmPass.PasswordChar == '*')
            {
                btnHideConfirm.BringToFront();
                txtConfirmPass.PasswordChar = '\0';
            }
        }
    }
}