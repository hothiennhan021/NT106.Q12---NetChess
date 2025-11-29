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

        private void btnRemove_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem đã chọn ai chưa
            if (lbFriends.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một người bạn để xóa!");
                return;
            }

            // 2. Lấy tên người bạn từ dòng đã chọn
  
            string selectedText = lbFriends.SelectedItem.ToString();

            // Cắt lấy phần Tên (trước dấu cách đầu tiên)
            
            string friendName = selectedText.Split(' ')[0];

            // 3. Hiện hộp thoại xác nhận (Theo Flow)
            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa '{friendName}' khỏi danh sách bạn bè không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                // 4. Gửi lệnh lên Server
                string response = ClientSocket.SendAndReceive($"FRIEND_REMOVE|{friendName}");

                if (response.Contains("SUCCESS"))
                {
                    MessageBox.Show("Đã xóa thành công!");
                    // Tải lại danh sách để cập nhật giao diện
                    LoadFriendList();
                }
                else
                {
                    MessageBox.Show("Lỗi: " + response);
                }
            }
        }
    }
}