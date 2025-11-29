using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using ChessClient;

namespace AccountUI
{
    public partial class MainMenu : Form
    {
        private bool _isListening = false;

        public MainMenu()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_isListening) return;
            try
            {
                button1.Text = "Đang tìm...";
                button1.Enabled = false;
                button3.Enabled = false;
                _isListening = true;

                if (!ClientManager.Instance.IsConnected)
                {
                    MessageBox.Show("Mất kết nối! Vui lòng đăng nhập lại.");
                    this.Close();
                    return;
                }

                await ClientManager.Instance.SendAsync("FIND_GAME");
                await ListenForGameStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
                ResetUI();
            }
        }

        private async Task ListenForGameStart()
        {
            await Task.Run(() =>
            {
                while (_isListening)
                {
                    try
                    {
                        string message = ClientManager.Instance.WaitForMessage();
                        if (message == null)
                        {
                            this.Invoke((MethodInvoker)(() => { MessageBox.Show("Mất kết nối."); this.Close(); }));
                            break;
                        }

                        if (message.StartsWith("GAME_START"))
                        {
                            _isListening = false;
                            LaunchWpfGameWindow(message);
                            break;
                        }
                        else if (message.StartsWith("WAITING"))
                        {
                            this.Invoke((MethodInvoker)(() => button1.Text = "Đang đợi đối thủ..."));
                        }
                    }
                    catch { _isListening = false; }
                }
            });
        }

        private void LaunchWpfGameWindow(string gameStartMessage)
        {
            Thread wpfThread = new Thread(() =>
            {
                try
                {
                    ChessUI.MainWindow gameWindow = new ChessUI.MainWindow(gameStartMessage);

                    gameWindow.Loaded += (s, e) => this.BeginInvoke((MethodInvoker)(() => this.Hide()));

                    gameWindow.Closed += (s, e) =>
                    {
                        // --- ĐẢM BẢO MENU HIỆN TRƯỚC ---
                        try
                        {
                            if (!this.IsDisposed && this.IsHandleCreated)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    this.Show();
                                    this.WindowState = FormWindowState.Normal;
                                    this.BringToFront();
                                    ResetUI();
                                });
                            }
                        }
                        catch { }

                        // Tắt luồng WPF sau cùng
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    };

                    gameWindow.Show();
                    Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    this.BeginInvoke((MethodInvoker)(() => { MessageBox.Show("Lỗi Game: " + ex.Message); this.Show(); ResetUI(); }));
                }
            });

            wpfThread.SetApartmentState(ApartmentState.STA);
            wpfThread.IsBackground = false;
            wpfThread.Start();
        }

        private void ResetUI()
        {
            _isListening = false;
            button1.Text = "Ghép trận";
            button1.Enabled = true;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e) { MessageBox.Show("Chưa có tính năng này."); }
        private void button4_Click(object sender, EventArgs e) { ClientManager.Disconnect(); Application.Exit(); }
        private void MainMenu_Load(object sender, EventArgs e) { }

        private void btnFriend_Click(object sender, EventArgs e)
        {
            Friend frm = new Friend();
            frm.ShowDialog();
        }
    }
}