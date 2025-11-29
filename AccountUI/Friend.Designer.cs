namespace AccountUI
{
    partial class Friend
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabList = new TabPage();
            lbFriends = new ListBox();
            btnRefresh = new Button();
            tabSearch = new TabPage();
            btnRefreshRequest = new Button();
            label2 = new Label();
            btnAccept = new Button();
            lbRequests = new ListBox();
            btnSearch = new Button();
            txtSearch = new TextBox();
            label1 = new Label();
            btnRemove = new Button();
            tabControl1.SuspendLayout();
            tabList.SuspendLayout();
            tabSearch.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabList);
            tabControl1.Controls.Add(tabSearch);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(3, 4, 3, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(553, 615);
            tabControl1.TabIndex = 0;
            // 
            // tabList
            // 
            tabList.Controls.Add(btnRemove);
            tabList.Controls.Add(lbFriends);
            tabList.Controls.Add(btnRefresh);
            tabList.Location = new Point(4, 29);
            tabList.Margin = new Padding(3, 4, 3, 4);
            tabList.Name = "tabList";
            tabList.Padding = new Padding(3, 4, 3, 4);
            tabList.Size = new Size(545, 582);
            tabList.TabIndex = 0;
            tabList.Text = "Danh sách bạn bè";
            tabList.UseVisualStyleBackColor = true;
            // 
            // lbFriends
            // 
            lbFriends.FormattingEnabled = true;
            lbFriends.Location = new Point(8, 23);
            lbFriends.Name = "lbFriends";
            lbFriends.Size = new Size(526, 424);
            lbFriends.TabIndex = 2;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(8, 454);
            btnRefresh.Margin = new Padding(3, 4, 3, 4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(526, 53);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "Làm mới danh sách";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click_1;
            // 
            // tabSearch
            // 
            tabSearch.Controls.Add(btnRefreshRequest);
            tabSearch.Controls.Add(label2);
            tabSearch.Controls.Add(btnAccept);
            tabSearch.Controls.Add(lbRequests);
            tabSearch.Controls.Add(btnSearch);
            tabSearch.Controls.Add(txtSearch);
            tabSearch.Controls.Add(label1);
            tabSearch.Location = new Point(4, 29);
            tabSearch.Margin = new Padding(3, 4, 3, 4);
            tabSearch.Name = "tabSearch";
            tabSearch.Padding = new Padding(3, 4, 3, 4);
            tabSearch.Size = new Size(545, 582);
            tabSearch.TabIndex = 1;
            tabSearch.Text = "Tìm kiếm & Lời mời";
            tabSearch.UseVisualStyleBackColor = true;
            // 
            // btnRefreshRequest
            // 
            btnRefreshRequest.Location = new Point(253, 95);
            btnRefreshRequest.Margin = new Padding(3, 4, 3, 4);
            btnRefreshRequest.Name = "btnRefreshRequest";
            btnRefreshRequest.Size = new Size(137, 33);
            btnRefreshRequest.TabIndex = 6;
            btnRefreshRequest.Text = "làm mới";
            btnRefreshRequest.UseVisualStyleBackColor = true;
            btnRefreshRequest.Click += btnRefreshRequest_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 101);
            label2.Name = "label2";
            label2.Size = new Size(172, 20);
            label2.TabIndex = 5;
            label2.Text = "Lời mời kết bạn đã nhận:";
            // 
            // btnAccept
            // 
            btnAccept.Location = new Point(9, 441);
            btnAccept.Margin = new Padding(3, 4, 3, 4);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(526, 53);
            btnAccept.TabIndex = 4;
            btnAccept.Text = "Đồng ý kết bạn";
            btnAccept.UseVisualStyleBackColor = true;
            btnAccept.Click += btnAccept_Click;
            // 
            // lbRequests
            // 
            lbRequests.FormattingEnabled = true;
            lbRequests.Location = new Point(10, 134);
            lbRequests.Margin = new Padding(3, 4, 3, 4);
            lbRequests.Name = "lbRequests";
            lbRequests.Size = new Size(525, 284);
            lbRequests.TabIndex = 3;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(398, 47);
            btnSearch.Margin = new Padding(3, 4, 3, 4);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(137, 33);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Gửi lời mời";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(9, 48);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(381, 27);
            txtSearch.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 20);
            label1.Name = "label1";
            label1.Size = new Size(148, 20);
            label1.TabIndex = 0;
            label1.Text = "Nhập tên người chơi:";
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(8, 393);
            btnRemove.Margin = new Padding(3, 4, 3, 4);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(526, 53);
            btnRemove.TabIndex = 3;
            btnRemove.Text = "Xóa bạn";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // Friend
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(553, 615);
            Controls.Add(tabControl1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Friend";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bạn bè";
            Load += FriendForm_Load;
            tabControl1.ResumeLayout(false);
            tabList.ResumeLayout(false);
            tabSearch.ResumeLayout(false);
            tabSearch.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabList;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.TabPage tabSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.ListBox lbRequests;
        private Button btnRefreshRequest;
        private ListBox lbFriends;
        private Button btnRemove;
    }
}