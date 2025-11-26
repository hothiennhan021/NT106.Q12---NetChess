using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents; // Dùng cho Chat (RichTextBox)
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessClient; // Project ChessClient
using ChessLogic;  // Project ChessLogic (GameState, Board, Timer...)

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        // --- UI COMPONENTS ---
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        // --- GAME STATE ---
        private GameState _localGameState;
        private Position selectedPos = null;
        private Player _myColor;

        // --- NETWORK & SYSTEM ---
        private NetworkClient _networkClient;
        private bool _isNavigatingToMenu = false; // Cờ kiểm soát điều hướng an toàn (của bạn)
        private Action _showMenuAction;           // Hàm quay về Menu (của bạn)

        // --- FEATURES (Chat & Timer - của bạn bạn) ---
        private ChessTimer _gameTimer;

        // Constructor nhận cả message khởi tạo và action thoát
        public MainWindow(string gameStartMessage, Action onExit)
        {
            InitializeComponent();
            _showMenuAction = onExit;

            InitializedBoard(); // Vẽ bàn cờ trắng

            try
            {
                _networkClient = ClientManager.Instance;
                if (!_networkClient.IsConnected) throw new Exception("Mất kết nối Client.");

                // 1. Khởi tạo Timer (Mặc định 10p, sẽ sync ngay lập tức)
                _gameTimer = new ChessTimer(10);
                _gameTimer.Tick += OnTimerTick; // Đăng ký sự kiện nhảy số

                // 2. Xử lý tin nhắn GAME_START ngay lập tức
                HandleServerMessage(gameStartMessage);

                // 3. Bắt đầu lắng nghe tin nhắn mới
                StartServerListener();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi động: {ex.Message}");
                this.Close();
            }
        }

        // --- PHẦN 1: MẠNG & XỬ LÝ TIN NHẮN (GỘP LOGIC) ---

        private void StartServerListener()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        string msg = _networkClient.WaitForMessage();
                        if (msg == null)
                        {
                            Dispatcher.Invoke(OnDisconnected);
                            break;
                        }
                        // Đưa về luồng UI để xử lý an toàn
                        Dispatcher.Invoke(() => HandleServerMessage(msg));
                    }
                }
                catch { }
            });
        }

        private void OnDisconnected()
        {
            if (_isNavigatingToMenu) return; // Nếu đang chủ động thoát thì thôi
            MessageBox.Show("Mất kết nối đến Server!", "Lỗi mạng");
            this.Close();
        }

        // HÀM XỬ LÝ TRUNG TÂM (QUAN TRỌNG NHẤT)
        private void HandleServerMessage(string message)
        {
            string[] parts = message.Split('|');
            string command = parts[0];

            switch (command)
            {
                case "GAME_START":
                    // Format: GAME_START|COLOR|BOARD|W_TIME|B_TIME
                    SetupGame(parts);
                    break;

                case "UPDATE":
                    // Format: UPDATE|BOARD|CURRENT_PLAYER|W_TIME|B_TIME
                    UpdateGame(parts);
                    break;

                case "CHAT":
                    // Format: CHAT|CONTENT
                    if (parts.Length > 1) AppendChatMessage("Đối thủ", parts[1]);
                    break;

                // --- CÁC LỆNH TƯƠNG TÁC (Logic của bạn) ---
                case "GAME_OVER":
                    _gameTimer.Stop();
                    MessageBox.Show(parts[1], "Kết thúc");
                    if (!IsMenuOnScreen()) ShowGameOver();
                    break;

                case "ASK_RESTART":
                    HandleRestartRequest();
                    break;

                case "RESTART_DENIED":
                    MessageBox.Show("Đối thủ từ chối chơi lại.");
                    if (!IsMenuOnScreen()) ShowGameOver();
                    break;

                case "OPPONENT_LEFT":
                    MessageBox.Show("Đối thủ đã thoát. Bạn sẽ về Menu chính.");
                    ReturnToMainMenu();
                    break;

                case "ERROR":
                    MessageBox.Show(parts[1], "Lỗi");
                    break;
            }
        }

        // --- CÁC HÀM XỬ LÝ LOGIC CHI TIẾT ---

        private void SetupGame(string[] parts)
        {
            // Ẩn menu cũ nếu có
            MenuContainer.Content = null;

            _myColor = (parts[1] == "WHITE") ? Player.White : Player.Black;
            this.Title = $"Cờ Vua - Bạn là quân {_myColor}";

            Board board = Serialization.ParseBoardString(parts[2]);
            _localGameState = new GameState(Player.White, board);

            DrawBoard(_localGameState.Board);
            SetCursor(_localGameState.CurrentPlayer);

            // Xử lý Timer (Phần code của bạn bạn)
            if (parts.Length >= 5)
            {
                int wTime = int.Parse(parts[3]);
                int bTime = int.Parse(parts[4]);
                _gameTimer.Sync(wTime, bTime);
                _gameTimer.Start(Player.White);
                UpdateTimerColors();
            }
        }

        private void UpdateGame(string[] parts)
        {
            Board board = Serialization.ParseBoardString(parts[1]);
            Player currentPlayer = (parts[2] == "WHITE") ? Player.White : Player.Black;

            _localGameState = new GameState(currentPlayer, board);

            DrawBoard(_localGameState.Board);
            SetCursor(_localGameState.CurrentPlayer);

            // Sync Timer
            if (parts.Length >= 5)
            {
                int wTime = int.Parse(parts[3]);
                int bTime = int.Parse(parts[4]);
                _gameTimer.Sync(wTime, bTime);
                _gameTimer.Start(currentPlayer);
                UpdateTimerColors();
            }
        }

        private void HandleRestartRequest()
        {
            var result = MessageBox.Show("Đối thủ muốn chơi lại. Đồng ý?", "Yêu cầu", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                _ = _networkClient.SendAsync("REQUEST_RESTART");
            else
                _ = _networkClient.SendAsync("RESTART_NO");
        }

        // --- PHẦN 2: CHAT & TIMER UI (CỦA BẠN BẠN) ---

        private void OnTimerTick(int wTime, int bTime)
        {
            // Timer chạy thread riêng nên phải Invoke
            Dispatcher.Invoke(() =>
            {
                lblWhiteTime.Text = TimeSpan.FromSeconds(wTime).ToString(@"mm\:ss");
                lblBlackTime.Text = TimeSpan.FromSeconds(bTime).ToString(@"mm\:ss");
            });
        }

        private void UpdateTimerColors()
        {
            if (_localGameState.CurrentPlayer == Player.White)
            {
                lblWhiteTime.Foreground = Brushes.Red;
                lblBlackTime.Foreground = Brushes.White;
            }
            else
            {
                lblWhiteTime.Foreground = Brushes.White;
                lblBlackTime.Foreground = Brushes.Red;
            }
        }

        private async void btnSendChat_Click(object sender, RoutedEventArgs e)
        {
            string content = txtChatInput.Text.Trim();
            if (!string.IsNullOrEmpty(content))
            {
                await _networkClient.SendAsync($"CHAT|{content}");
                AppendChatMessage("Tôi", content);
                txtChatInput.Text = "";
            }
        }

        private void txtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnSendChat_Click(sender, e);
        }

        private void AppendChatMessage(string sender, string message)
        {
            Paragraph p = new Paragraph();
            Run rSender = new Run(sender + ": ") { FontWeight = FontWeights.Bold, Foreground = Brushes.Orange };
            if (sender == "Tôi") rSender.Foreground = Brushes.LightBlue;

            p.Inlines.Add(rSender);
            p.Inlines.Add(new Run(message) { Foreground = Brushes.White });

            txtChatHistory.Document.Blocks.Add(p);
            txtChatHistory.ScrollToEnd();
        }

        // --- PHẦN 3: LOGIC THOÁT & MENU (CỦA BẠN) ---

        private void ShowGameOver()
        {
            GameOverMenu menu = new GameOverMenu(_localGameState); // UserControl của bạn
            MenuContainer.Content = menu;

            menu.OptionSelected += option =>
            {
                if (option == Option.Restart) // Option là enum trong file GameOverMenu của bạn
                {
                    _ = _networkClient.SendAsync("REQUEST_RESTART");
                    // Hiện thông báo chờ tạm thời
                    MenuContainer.Content = new TextBlock { Text = "Đang chờ đối thủ...", Foreground = Brushes.White, FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                }
                else
                {
                    QuitGameSafe();
                }
            };
        }

        private async void QuitGameSafe()
        {
            try { await _networkClient.SendAsync("LEAVE_GAME"); } catch { }
            ReturnToMainMenu();
        }

        private void ReturnToMainMenu()
        {
            _isNavigatingToMenu = true;
            Dispatcher.Invoke(() =>
            {
                _showMenuAction?.Invoke();
                this.Close();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isNavigatingToMenu) _networkClient.CloseConnection();
        }

        private bool IsMenuOnScreen() => MenuContainer.Content != null;

        // --- PHẦN 4: VẼ BÀN CỜ & DI CHUYỂN (GIỮ NGUYÊN) ---

        private void InitializedBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image img = new Image();
                    pieceImages[r, c] = img;
                    PieceGrid.Children.Add(img);

                    Rectangle rect = new Rectangle();
                    highlights[r, c] = rect;
                    HighlightGrid.Children.Add(rect);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = (_myColor == Player.Black) ? new Position(7 - r, 7 - c) : new Position(r, c);
                    pieceImages[r, c].Source = Images.GetImage(board[pos]);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen()) return;
            Point p = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(p);

            if (selectedPos == null) OnFromPositionSelected(pos);
            else OnToPositionSelected(pos);
        }

        private void OnFromPositionSelected(Position pos)
        {
            if (_localGameState.CurrentPlayer != _myColor) return;
            var moves = _localGameState.MovesForPiece(pos);
            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();
            if (moveCache.TryGetValue(pos, out Move move))
            {
                // Kiểm tra hợp lệ trước khi gửi
                if (!move.IsLegal(_localGameState.Board)) return;
                _ = _networkClient.SendAsync($"MOVE|{move.FromPos.Row}|{move.FromPos.Column}|{move.ToPos.Row}|{move.ToPos.Column}");
            }
        }

        private Position ToSquarePosition(Point p)
        {
            double size = BoardGrid.ActualWidth / 8;
            int r = (int)(p.Y / size);
            int c = (int)(p.X / size);
            if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; }
            return new Position(r, c);
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (var m in moves) moveCache[m.ToPos] = m;
        }

        private void ShowHighlights()
        {
            foreach (var p in moveCache.Keys)
            {
                int r = p.Row, c = p.Column;
                if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; }
                highlights[r, c].Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
            }
        }

        private void HideHighlights()
        {
            foreach (var rect in highlights) rect.Fill = Brushes.Transparent;
        }

        private void SetCursor(Player p)
        {
            Cursor = (p == Player.White) ? ChessCursors.WhiteCursor : ChessCursors.BlackCursor;
        }
    }
}