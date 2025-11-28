namespace AccountUI
{
    partial class MainMenu
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            btnFriend = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.BackColor = Color.DarkSlateBlue;
            button1.Font = new Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 163);
            button1.ForeColor = Color.White;
            button1.Location = new Point(370, 293);
            button1.Name = "button1";
            button1.Size = new Size(224, 54);
            button1.TabIndex = 33;
            button1.Text = "Ghép trận";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(789, 368);
            button2.Name = "button2";
            button2.Size = new Size(8, 8);
            button2.TabIndex = 34;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.AutoSize = true;
            button3.BackColor = Color.DarkSlateBlue;
            button3.Font = new Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 163);
            button3.ForeColor = Color.White;
            button3.Location = new Point(370, 167);
            button3.Name = "button3";
            button3.Size = new Size(224, 54);
            button3.TabIndex = 35;
            button3.Text = "Tạo Phòng";
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.AutoSize = true;
            button4.BackColor = Color.DarkSlateBlue;
            button4.Font = new Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 163);
            button4.ForeColor = Color.White;
            button4.Location = new Point(789, 440);
            button4.Name = "button4";
            button4.Size = new Size(142, 54);
            button4.TabIndex = 36;
            button4.Text = "Đăng Xuất";
            button4.UseVisualStyleBackColor = false;
            // 
            // btnFriend
            // 
            btnFriend.AutoSize = true;
            btnFriend.BackColor = Color.DarkSlateBlue;
            btnFriend.Font = new Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 163);
            btnFriend.ForeColor = Color.White;
            btnFriend.Location = new Point(38, 440);
            btnFriend.Name = "btnFriend";
            btnFriend.Size = new Size(177, 54);
            btnFriend.TabIndex = 37;
            btnFriend.Text = "Bạn bè";
            btnFriend.UseVisualStyleBackColor = false;
            btnFriend.Click += btnFriend_Click;
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.BackGround;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(962, 521);
            Controls.Add(btnFriend);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "MainMenu";
            Text = "MainMenu";
            Load += MainMenu_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button btnFriend;
    }
}