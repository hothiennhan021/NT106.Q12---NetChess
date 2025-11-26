#nullable disable
using ChessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        private GameState _localGameState;
        private Position selectedPos = null;

        private NetworkClient _networkClient;
        private Player _myColor;

        private Task _listenerTask;
        private Action _showMenuAction;
        // [SỬA 1] Biến cờ để kiểm soát việc chuyển màn hình
        private bool _isNavigatingToMenu = false;

        public MainWindow(string gameStartMessage, Action onExit)
        {
            InitializeComponent();
            _showMenuAction = onExit;
            InitializedBoard();

            try
            {
                // 1. Lấy kết nối từ ClientManager
                _networkClient = ClientManager.Instance;
                if (!_networkClient.IsConnected)
                {
                    throw new Exception("Client không được kết nối.");
                }

                // 2. Xử lý tin nhắn khởi tạo (GAME_START) ngay lập tức
                HandleServerMessage(gameStartMessage);

                // 3. Bắt đầu luồng lắng nghe tin nhắn mới
                StartServerListener();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi động game: {ex.Message}", "Lỗi");
                this.Close();
            }
        }

        private void StartServerListener()
        {
            _listenerTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        string message = _networkClient.WaitForMessage();
                        if (message == null)
                        {
                            Dispatcher.Invoke(OnDisconnected);
                            break;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                HandleServerMessage(message);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[MainWindow Error] {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Listener stopped: " + ex.Message);
                }
            });
        }

        private void OnDisconnected()
        {
            // Nếu đang chủ động chuyển về Menu thì không cần báo lỗi mất kết nối
            if (_isNavigatingToMenu) return;

            MessageBox.Show("Mất kết nối đến server.", "Mất kết nối");
            this.Close();
        }

        // --- HÀM XỬ LÝ TIN NHẮN TỪ SERVER ---
        private void HandleServerMessage(string message)
        {
            var parts = message.Split('|');
            var command = parts[0];

            switch (command)
            {
                case "GAME_START":
                    // Ẩn menu Game Over/Màn hình chờ nếu đang hiện
                    MenuContainer.Content = null;

                    _myColor = (parts[1] == "WHITE") ? Player.White : Player.Black;
                    Board board = Serialization.ParseBoardString(parts[2]);

                    _localGameState = new GameState(Player.White, board);

                    DrawBoard(_localGameState.Board);
                    SetCursor(_localGameState.CurrentPlayer);
                    this.Title = $"Game Cờ Vua - Bạn là quân {parts[1]}";
                    break;

                case "UPDATE":
                    Console.WriteLine("[INFO] Nhận UPDATE bàn cờ từ Server.");
                    Board updatedBoard = Serialization.ParseBoardString(parts[1]);
                    Player currentPlayer = (parts[2] == "WHITE") ? Player.White : Player.Black;

                    _localGameState = new GameState(currentPlayer, updatedBoard);

                    DrawBoard(_localGameState.Board);
                    SetCursor(_localGameState.CurrentPlayer);

                    if (_localGameState.IsGameOver())
                    {
                        if (!IsMenuOnScreen()) ShowGameOver();
                    }
                    break;

                case "ERROR":
                    MessageBox.Show(parts[1], "Lỗi từ Server");
                    break;

                case "GAME_OVER":
                    MessageBox.Show(parts[1], "Trò chơi kết thúc");
                    if (!IsMenuOnScreen()) ShowGameOver();
                    break;

                // --- XỬ LÝ YÊU CẦU CHƠI LẠI ---
                case "ASK_RESTART":
                    var result = MessageBox.Show("Đối thủ muốn chơi ván mới. Bạn có đồng ý không?",
                                                 "Yêu cầu chơi lại",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Task.Run(async () => await _networkClient.SendAsync("REQUEST_RESTART"));
                    }
                    else
                    {
                        Task.Run(async () => await _networkClient.SendAsync("RESTART_NO"));
                    }
                    break;

                case "RESTART_DENIED":
                    MessageBox.Show("Đối thủ đã từ chối chơi lại.", "Thông báo");
                    if (!IsMenuOnScreen()) ShowGameOver();
                    break;
                // Trong file MainWindow.xaml.cs -> HandleServerMessage()

                case "OPPONENT_LEFT":
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Đối thủ đã thoát khỏi trò chơi.\nBạn sẽ trở về màn hình chính.",
                                        "Kết thúc",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                        // [SỬA] Gọi hàm chung đã viết ở Bước 1
                        ReturnToMainMenu();
                    });
                    break;

                case "WAITING":
                    break;
            }
        }

        // --- HÀM HIỂN THỊ MENU KẾT THÚC & XỬ LÝ NÚT BẤM ---
        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(_localGameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    Task.Run(async () => await _networkClient.SendAsync("REQUEST_RESTART"));
                    // Hiển thị chờ (Code hiển thị chờ của bạn ở đây...)
                    TextBlock waitingText = new TextBlock { Text = "Đang chờ đối thủ...", FontSize = 30, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    MenuContainer.Content = new Border { Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), Child = waitingText };
                }
                else // Exit
                {
                    QuitGameSafe();
                }
            };
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // [SỬA 4] Chỉ đóng kết nối nếu KHÔNG PHẢI là đang chuyển về menu
            // Nếu bấm X đỏ -> _isNavigatingToMenu = false -> Cắt mạng (Đúng)
            // Nếu bấm Exit -> _isNavigatingToMenu = true -> Giữ mạng (Đúng)
            if (_isNavigatingToMenu == false)
            {
                if (_networkClient != null)
                {
                    _networkClient.CloseConnection();
                }
            }
        }
        // Trong file MainWindow.xaml.cs

        // Trong MainWindow.xaml.cs

        private async void QuitGameSafe()
        {
            try
            {
                // 1. Gửi tin nhắn báo Server là tôi chủ động thoát
                if (_networkClient != null && _networkClient.IsConnected)
                {
                    await _networkClient.SendAsync("LEAVE_GAME");
                }
            }
            catch
            {
                // Lờ đi lỗi nếu mạng đã mất
            }
            finally
            {
                // 2. Sau khi gửi xong (hoặc lỗi), tiến hành đóng Form và về Menu
                ReturnToMainMenu();
            }
        }

        // Hàm về Menu (đã sửa ở bước trước, nhắc lại để chắc chắn)
        private void ReturnToMainMenu()
        {
            _isNavigatingToMenu = true; // Cờ này quan trọng để chặn sự kiện Closing cắt mạng lần 2

            // Đảm bảo chạy trên luồng UI chính
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_showMenuAction != null) _showMenuAction.Invoke();
                this.Close();
            });
        }
     

        #region Logic Game & UI (Giữ nguyên)

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen()) return;
            if (_myColor == Player.None) return;

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private void OnFromPositionSelected(Position pos)
        {
            if (_localGameState.CurrentPlayer != _myColor) return;

            IEnumerable<Move> moves = _localGameState.MovesForPiece(pos);
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
                HandleMove(move);
            }
        }

        private void HandleMove(Move move)
        {
            if (_localGameState.CurrentPlayer != _myColor) return;

            Task.Run(async () =>
            {
                try
                {
                    bool isLegal = move.IsLegal(_localGameState.Board);
                    if (!isLegal)
                    {
                        Dispatcher.Invoke(() => MessageBox.Show("Nước đi không hợp lệ! (Tự chiếu tướng?)"));
                        return;
                    }

                    string moveString = $"MOVE|{move.FromPos.Row}|{move.FromPos.Column}|{move.ToPos.Row}|{move.ToPos.Column}";
                    await _networkClient.SendAsync(moveString);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => MessageBox.Show($"Lỗi gửi nước đi: {ex.Message}"));
                }
            });
        }

        private void InitializedBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position boardPos = (_myColor == Player.Black) ? new Position(7 - r, 7 - c) : new Position(r, c);
                    Pieces piece = board[boardPos];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);

            if (_myColor == Player.Black)
            {
                row = 7 - row;
                col = 7 - col;
            }
            return new Position(row, col);
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(159, 125, 255, 125);
            foreach (Position to in moveCache.Keys)
            {
                int r = to.Row, c = to.Column;
                if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; }
                highlights[r, c].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                int r = to.Row, c = to.Column;
                if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; }
                highlights[r, c].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            Cursor = (player == Player.White) ? ChessCursors.WhiteCursor : ChessCursors.BlackCursor;
        }
        #endregion
    }
}