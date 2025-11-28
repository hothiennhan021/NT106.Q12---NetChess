using System;
using System.Windows.Forms;

namespace AccountUI
{
    public partial class Friend : Form
    {
        public Friend()
        {
            InitializeComponent();
        }

        // 1. Sự kiện khi Form vừa mở lên -> Tự động tải danh sách
        private void FriendForm_Load(object sender, EventArgs e)
        {
            LoadFriendList();
            LoadFriendRequests();
        }

        // 2. Hàm hỗ trợ: Tải danh sách bạn bè từ Server
        // Hàm tải danh sách bạn bè (Clean Version)
        private void LoadFriendList()
        {
            try
            {
                string response = ClientSocket.SendAndReceive("FRIEND_GET_LIST");

                if (response != null && response.StartsWith("FRIEND_LIST|"))
                {
                    lbFriends.Items.Clear();

                    int firstSplitIndex = response.IndexOf('|');
                    if (firstSplitIndex == -1) return;

                    // Cắt header và làm sạch chuỗi
                    string data = response.Substring(firstSplitIndex + 1)
                                          .Replace("\0", "")
                                          .Trim()
                                          .TrimEnd(';');

                    if (string.IsNullOrWhiteSpace(data)) return;

                    string[] listFriends = data.Split(';');

                    foreach (string item in listFriends)
                    {
                        if (string.IsNullOrWhiteSpace(item)) continue;

                        string[] info = item.Split('|');

                        // Chỉ hiển thị nếu có đủ Tên và Elo
                        if (info.Length >= 2)
                        {
                            string name = info[0];
                            string elo = info[1];
                            string online = (info.Length > 2 && info[2].ToLower() == "true") ? "Online" : "Offline";

                            lbFriends.Items.Add($"{name} (Elo: {elo}) - [{online}]");
                        }
                    }
                }
            }
            catch { /* Lờ lỗi để không làm phiền người dùng */ }
        }

        // Hàm tải lời mời (Clean Version)
        private void LoadFriendRequests()
        {
            try
            {
                string response = ClientSocket.SendAndReceive("FRIEND_GET_REQUESTS");

                if (response != null && response.StartsWith("FRIEND_REQUESTS|"))
                {
                    lbRequests.Items.Clear();

                    int firstSplitIndex = response.IndexOf('|');
                    if (firstSplitIndex == -1) return;

                    string data = response.Substring(firstSplitIndex + 1)
                                          .Replace("\0", "")
                                          .Trim()
                                          .TrimEnd(';');

                    if (string.IsNullOrWhiteSpace(data)) return;

                    string[] reqs = data.Split(';');
                    foreach (var r in reqs)
                    {
                        if (!string.IsNullOrWhiteSpace(r))
                        {
                            // r dạng: "1|trung123"
                            lbRequests.Items.Add(r);
                        }
                    }
                }
            }
            catch { }
        }

        // --- CÁC NÚT BẤM (BUTTON EVENTS) ---

        // Nút LÀM MỚI (Refresh)
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadFriendList();
            LoadFriendRequests();
        }

        // Nút GỬI LỜI MỜI (Search & Add)
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên người chơi cần tìm!");
                return;
            }

            // Gửi lệnh tìm kiếm lên Server
            string response = ClientSocket.SendAndReceive($"FRIEND_SEARCH|{name}");

            // Xử lý các trường hợp Server trả về
            if (response.Contains("SUCCESS"))
            {
                MessageBox.Show($"Đã gửi lời mời kết bạn tới {name} thành công!");
                txtSearch.Clear(); // Xóa ô nhập cho sạch
            }
            else if (response.Contains("NOT_FOUND"))
            {
                MessageBox.Show("Không tìm thấy người chơi này.");
            }
            else if (response.Contains("SELF_ERROR"))
            {
                MessageBox.Show("Bạn không thể tự kết bạn với chính mình.");
            }
            else if (response.Contains("EXISTED"))
            {
                MessageBox.Show("Người này đã là bạn hoặc đã có lời mời đang chờ.");
            }
            else
            {
                MessageBox.Show("Lỗi từ Server: " + response);
            }
        }

        // Nút ĐỒNG Ý KẾT BẠN (Accept)
        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (lbRequests.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một lời mời trong danh sách!");
                return;
            }

            try
            {
                // Item trong ListBox có dạng "15|HungNguyen" -> Cần cắt lấy số 15 (ID)
                string selected = lbRequests.SelectedItem.ToString();
                string reqId = selected.Split('|')[0]; // Lấy phần trước dấu |

                // Gửi lệnh đồng ý lên Server
                string response = ClientSocket.SendAndReceive($"FRIEND_ACCEPT|{reqId}");

                if (response == "FRIEND_ACCEPT_OK")
                {
                    MessageBox.Show("Đã chấp nhận kết bạn!");

                    // Tải lại cả 2 danh sách để cập nhật thay đổi
                    LoadFriendList();
                    LoadFriendRequests();
                }
                else
                {
                    MessageBox.Show("Lỗi: " + response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message);
            }
        }

        private void btnRefreshRequest_Click(object sender, EventArgs e)
        {
            LoadFriendRequests();
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            LoadFriendList();
        }

        // 1. Sự kiện bấm nút "Mời chơi cờ"
        private void btnInvite_Click(object sender, EventArgs e)
        {
            if (lbFriends.SelectedItem == null)
            {
                MessageBox.Show("Hãy chọn một người bạn đang Online để mời!");
                return;
            }

            // ListBox hiện: "trung123 (Elo: 1000) - [Online]"
            string selected = lbFriends.SelectedItem.ToString();

            // Kiểm tra xem bạn đó có Online không
            if (!selected.Contains("[Online]"))
            {
                MessageBox.Show("Người này đang Offline, không mời được đâu!");
                return;
            }

            // Cắt lấy tên (Lấy chữ đầu tiên trước dấu cách)
            string friendName = selected.Split(' ')[0];

            // Gửi lệnh mời
            string response = ClientSocket.SendAndReceive($"FRIEND_INVITE|{friendName}");

            if (response.Contains("SUCCESS"))
            {
                MessageBox.Show($"Đã gửi lời thách đấu tới {friendName}!");
            }
            else
            {
                MessageBox.Show("Lỗi: " + response);
            }
        }

        // 2. Sự kiện Timer (Tự động chạy mỗi 3 giây để check thư)
        // 1. Sự kiện Timer (Tự động chạy mỗi 3 giây để check thư)
        private void timerCheckMail_Tick(object sender, EventArgs e)
        {
            try
            {
                string response = ClientSocket.SendAndReceive("CHECK_MAIL");
                if (response == "EMPTY" || response.StartsWith("ERROR")) return;

                string[] parts = response.Split('|');
                string type = parts[0];
                string senderName = parts[1];

                // 1. KHI ĐƯỢC MỜI (Người nhận - Quân Đen)
                if (type == "INVITE")
                {
                    timerCheckMail.Stop();
                    DialogResult dr = MessageBox.Show(
                        $"{senderName} đang thách đấu bạn! Chiến không?",
                        "Thách đấu", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        // Gửi đồng ý
                        ClientSocket.SendAndReceive($"FRIEND_RESPONSE|{senderName}|ACCEPTED");

                        // --- [QUAN TRỌNG] MỞ BÀN CỜ LUÔN ---
                        MoBanCo(isWhite: false); // False = Quân Đen
                                                 // -----------------------------------
                    }
                    timerCheckMail.Start();
                }

                // 2. KHI ĐỐI PHƯƠNG ĐỒNG Ý (Người gửi - Quân Trắng)
                else if (type == "RESPONSE" && parts[2] == "ACCEPTED")
                {
                    timerCheckMail.Stop();
                    MessageBox.Show($"{senderName} đã chấp nhận! Vào game thôi!");

                    // --- [QUAN TRỌNG] MỞ BÀN CỜ LUÔN ---
                    MoBanCo(isWhite: true); // True = Quân Trắng
                                            // -----------------------------------

                    timerCheckMail.Start();
                }
            }
            catch { }
        }

        // 2. Hàm phụ để mở bàn cờ (Đã sửa để truyền Phe)
        private void MoBanCo(bool isWhite)
        {
            this.Hide(); // Ẩn form bạn bè đi

            try
            {
                // Logic: Người mời đi trước (WHITE), Người nhận đi sau (BLACK)
                string side = isWhite ? "WHITE" : "BLACK";

                // Khởi tạo bàn cờ từ Project ChessUI
                // Truyền "WHITE" hoặc "BLACK" vào để bàn cờ biết đường xếp quân
                ChessUI.MainWindow gameWindow = new ChessUI.MainWindow(side);

                gameWindow.ShowDialog(); // Mở lên và chờ chơi xong
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở bàn cờ (Kiểm tra lại Reference): " + ex.Message);
            }

            this.Show(); // Hiện lại form bạn bè sau khi chơi xong
        }
    }
}