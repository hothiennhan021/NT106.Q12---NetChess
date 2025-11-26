namespace AccountUI
{
    partial class Signup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Signup));
            this.pbIconUser = new System.Windows.Forms.PictureBox();
            this.pbIconPass = new System.Windows.Forms.PictureBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.pbIconConfirm = new System.Windows.Forms.PictureBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.pbIconEmail = new System.Windows.Forms.PictureBox();
            this.txtConfirmPass = new System.Windows.Forms.TextBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnHidePass = new System.Windows.Forms.Button();
            this.btnHideConfirm = new System.Windows.Forms.Button();
            this.btnShowPass = new System.Windows.Forms.Button();
            this.btnShowConfirm = new System.Windows.Forms.Button();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.dtpBirthday = new System.Windows.Forms.DateTimePicker();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconPass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconConfirm)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconEmail)).BeginInit();
            this.SuspendLayout();
            // 
            // pbIconEmail (Old: pictureBox1)
            // 
            this.pbIconEmail.BackColor = System.Drawing.Color.White;
            this.pbIconEmail.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image"))); // Giữ nguyên resource cũ
            this.pbIconEmail.Location = new System.Drawing.Point(650, 175);
            this.pbIconEmail.Margin = new System.Windows.Forms.Padding(4);
            this.pbIconEmail.Name = "pbIconEmail";
            this.pbIconEmail.Size = new System.Drawing.Size(49, 49);
            this.pbIconEmail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIconEmail.TabIndex = 19;
            this.pbIconEmail.TabStop = false;
            // 
            // pbIconPass (Old: pictureBox2)
            // 
            this.pbIconPass.BackColor = System.Drawing.Color.White;
            this.pbIconPass.Image = global::AccountUI.Properties.Resources.Password;
            this.pbIconPass.Location = new System.Drawing.Point(650, 385);
            this.pbIconPass.Margin = new System.Windows.Forms.Padding(4);
            this.pbIconPass.Name = "pbIconPass";
            this.pbIconPass.Size = new System.Drawing.Size(49, 49);
            this.pbIconPass.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIconPass.TabIndex = 18;
            this.pbIconPass.TabStop = false;
            // 
            // txtEmail (Old: textBox2)
            // 
            this.txtEmail.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.txtEmail.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtEmail.Location = new System.Drawing.Point(707, 175);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(4);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(493, 39);
            this.txtEmail.TabIndex = 1;
            this.txtEmail.Text = "Email";
            this.txtEmail.Enter += new System.EventHandler(this.TxtEmail_Enter);
            this.txtEmail.Leave += new System.EventHandler(this.TxtEmail_Leave);
            // 
            // txtUsername (Old: textBox1)
            // 
            this.txtUsername.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.txtUsername.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtUsername.Location = new System.Drawing.Point(707, 105);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(4);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(493, 39);
            this.txtUsername.TabIndex = 0;
            this.txtUsername.Text = "Tên Đăng Nhập";
            this.txtUsername.Enter += new System.EventHandler(this.TxtUsername_Enter);
            this.txtUsername.Leave += new System.EventHandler(this.TxtUsername_Leave);
            // 
            // pbIconConfirm (Old: pictureBox3)
            // 
            this.pbIconConfirm.BackColor = System.Drawing.Color.White;
            this.pbIconConfirm.Image = global::AccountUI.Properties.Resources.Password;
            this.pbIconConfirm.Location = new System.Drawing.Point(650, 455);
            this.pbIconConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.pbIconConfirm.Name = "pbIconConfirm";
            this.pbIconConfirm.Size = new System.Drawing.Size(49, 49);
            this.pbIconConfirm.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIconConfirm.TabIndex = 22;
            this.pbIconConfirm.TabStop = false;
            // 
            // txtPassword (Old: textBox3)
            // 
            this.txtPassword.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.txtPassword.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtPassword.Location = new System.Drawing.Point(707, 385);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(493, 39);
            this.txtPassword.TabIndex = 4;
            this.txtPassword.Text = "Mật Khẩu";
            this.txtPassword.Enter += new System.EventHandler(this.TxtPassword_Enter);
            this.txtPassword.Leave += new System.EventHandler(this.TxtPassword_Leave);
            // 
            // pbIconUser (Old: pictureBox4)
            // 
            this.pbIconUser.BackColor = System.Drawing.Color.White;
            this.pbIconUser.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pbIconUser.Location = new System.Drawing.Point(650, 105);
            this.pbIconUser.Margin = new System.Windows.Forms.Padding(4);
            this.pbIconUser.Name = "pbIconUser";
            this.pbIconUser.Size = new System.Drawing.Size(49, 49);
            this.pbIconUser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIconUser.TabIndex = 25;
            this.pbIconUser.TabStop = false;
            // 
            // txtConfirmPass (Old: textBox4)
            // 
            this.txtConfirmPass.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.txtConfirmPass.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtConfirmPass.Location = new System.Drawing.Point(707, 455);
            this.txtConfirmPass.Margin = new System.Windows.Forms.Padding(4);
            this.txtConfirmPass.Name = "txtConfirmPass";
            this.txtConfirmPass.Size = new System.Drawing.Size(493, 39);
            this.txtConfirmPass.TabIndex = 5;
            this.txtConfirmPass.Text = "Xác Nhận Mật Khẩu";
            this.txtConfirmPass.Enter += new System.EventHandler(this.TxtConfirmPass_Enter);
            this.txtConfirmPass.Leave += new System.EventHandler(this.TxtConfirmPass_Leave);
            // 
            // btnRegister (Old: button1)
            // 
            this.btnRegister.AutoSize = true;
            this.btnRegister.BackColor = System.Drawing.Color.DarkSlateBlue;
            this.btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = new System.Drawing.Font("Times New Roman", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.Location = new System.Drawing.Point(810, 530);
            this.btnRegister.Margin = new System.Windows.Forms.Padding(4);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(280, 68);
            this.btnRegister.TabIndex = 6;
            this.btnRegister.Text = "Đăng Ký";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
            // 
            // btnBack (Old: button2)
            // 
            this.btnBack.AutoSize = true;
            this.btnBack.BackColor = System.Drawing.Color.DarkSlateBlue;
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Times New Roman", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Location = new System.Drawing.Point(810, 610);
            this.btnBack.Margin = new System.Windows.Forms.Padding(4);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(280, 68);
            this.btnBack.TabIndex = 7;
            this.btnBack.Text = "Quay Lại";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Times New Roman", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(50, 250);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(573, 81);
            this.lblTitle.TabIndex = 28;
            this.lblTitle.Text = "CHESS ONLINE";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnHidePass
            // 
            this.btnHidePass.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHidePass.Image = ((System.Drawing.Image)(resources.GetObject("button_passwordhide.Image")));
            this.btnHidePass.Location = new System.Drawing.Point(1205, 385);
            this.btnHidePass.Margin = new System.Windows.Forms.Padding(4);
            this.btnHidePass.Name = "btnHidePass";
            this.btnHidePass.Size = new System.Drawing.Size(50, 50);
            this.btnHidePass.TabIndex = 29;
            this.btnHidePass.UseVisualStyleBackColor = true;
            this.btnHidePass.Click += new System.EventHandler(this.BtnHidePass_Click);
            // 
            // btnHideConfirm
            // 
            this.btnHideConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHideConfirm.Image = ((System.Drawing.Image)(resources.GetObject("button_passwordhide2.Image")));
            this.btnHideConfirm.Location = new System.Drawing.Point(1205, 454);
            this.btnHideConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnHideConfirm.Name = "btnHideConfirm";
            this.btnHideConfirm.Size = new System.Drawing.Size(50, 50);
            this.btnHideConfirm.TabIndex = 30;
            this.btnHideConfirm.UseVisualStyleBackColor = true;
            this.btnHideConfirm.Click += new System.EventHandler(this.BtnHideConfirm_Click);
            // 
            // btnShowPass
            // 
            this.btnShowPass.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowPass.Image = ((System.Drawing.Image)(resources.GetObject("button_passwordshow.Image")));
            this.btnShowPass.Location = new System.Drawing.Point(1205, 384);
            this.btnShowPass.Margin = new System.Windows.Forms.Padding(4);
            this.btnShowPass.Name = "btnShowPass";
            this.btnShowPass.Size = new System.Drawing.Size(50, 50);
            this.btnShowPass.TabIndex = 31;
            this.btnShowPass.UseVisualStyleBackColor = true;
            this.btnShowPass.Click += new System.EventHandler(this.BtnShowPass_Click);
            // 
            // btnShowConfirm
            // 
            this.btnShowConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowConfirm.Image = ((System.Drawing.Image)(resources.GetObject("button_passwordshow2.Image")));
            this.btnShowConfirm.Location = new System.Drawing.Point(1205, 454);
            this.btnShowConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnShowConfirm.Name = "btnShowConfirm";
            this.btnShowConfirm.Size = new System.Drawing.Size(50, 50);
            this.btnShowConfirm.TabIndex = 32;
            this.btnShowConfirm.UseVisualStyleBackColor = true;
            this.btnShowConfirm.Click += new System.EventHandler(this.BtnShowConfirm_Click);
            // 
            // txtFullName
            // 
            this.txtFullName.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.txtFullName.ForeColor = System.Drawing.SystemColors.GrayText;
            this.txtFullName.Location = new System.Drawing.Point(707, 245);
            this.txtFullName.Margin = new System.Windows.Forms.Padding(4);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new System.Drawing.Size(493, 39);
            this.txtFullName.TabIndex = 2;
            this.txtFullName.Text = "Họ và Tên";
            this.txtFullName.Enter += new System.EventHandler(this.TxtFullName_Enter);
            this.txtFullName.Leave += new System.EventHandler(this.TxtFullName_Leave);
            // 
            // dtpBirthday
            // 
            this.dtpBirthday.CalendarFont = new System.Drawing.Font("Times New Roman", 16.2F);
            this.dtpBirthday.Font = new System.Drawing.Font("Times New Roman", 16.2F);
            this.dtpBirthday.Location = new System.Drawing.Point(707, 315);
            this.dtpBirthday.Name = "dtpBirthday";
            this.dtpBirthday.Size = new System.Drawing.Size(493, 39);
            this.dtpBirthday.TabIndex = 3;
            // 
            // Signup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1300, 750);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pbIconUser);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.pbIconEmail);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.dtpBirthday);
            this.Controls.Add(this.pbIconPass);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.pbIconConfirm);
            this.Controls.Add(this.txtConfirmPass);
            this.Controls.Add(this.btnShowConfirm);
            this.Controls.Add(this.btnShowPass);
            this.Controls.Add(this.btnHideConfirm);
            this.Controls.Add(this.btnHidePass);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnRegister);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Signup";
            this.Text = "Đăng Ký";
            this.Load += new System.EventHandler(this.Signup_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbIconEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconPass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconConfirm)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbIconUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIconEmail;
        private System.Windows.Forms.PictureBox pbIconPass;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.PictureBox pbIconConfirm;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.PictureBox pbIconUser;
        private System.Windows.Forms.TextBox txtConfirmPass;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnHidePass;
        private System.Windows.Forms.Button btnHideConfirm;
        private System.Windows.Forms.Button btnShowPass;
        private System.Windows.Forms.Button btnShowConfirm;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.DateTimePicker dtpBirthday;
    }
}