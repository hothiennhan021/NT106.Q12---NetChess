using ChessClient;
using ChessLogic;
using ChessUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private bool _isExiting = false;
        private bool _isGameOver = false;

        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, List<Move>> moveCache = new Dictionary<Position, List<Move>>();

        private GameState _localGameState;
        private Position selectedPos = null;
        private Player _myColor;

        private NetworkClient _networkClient;
        private ServerResponseHandler _responseHandler;
        private ChessTimer _gameTimer;

        public MainWindow(string gameStartMessage)
        {
            InitializeComponent();
            LoadBoardImageSafe();
            InitializedBoard();

            _networkClient = ClientManager.Instance;
            _responseHandler = new ServerResponseHandler();
            _gameTimer = new ChessTimer(10);

            RegisterEvents();

            try
            {
                if (!_networkClient.IsConnected) throw new Exception("Mất kết nối.");
                _responseHandler.ProcessMessage(gameStartMessage);
                StartServerListener();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                Close();
            }
        }

        // --- NHẬN DỮ LIỆU PHÂN TÍCH VÀ MỞ CỬA SỔ MỚI ---
        private void OnAnalysisDataReceived(string moveHistoryString)
        {
            // Tách chuỗi lịch sử nước đi
            var moves = moveHistoryString.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            Dispatcher.Invoke(() => {
                // Ẩn menu kết thúc game
                MenuOverlay.Visibility = Visibility.Collapsed;

                // MỞ CỬA SỔ PHÂN TÍCH RIÊNG
                AnalysisWindow analysisWin = new AnalysisWindow(moves);
                analysisWin.Show();
            });
        }

        // ==========================================================
        // CÁC HÀM XỬ LÝ GAME PLAY (GIỮ NGUYÊN)
        // ==========================================================

        private void RegisterEvents()
        {
            _responseHandler.GameStarted += (s, e) => {
                _myColor = e.MyColor; _localGameState = new GameState(Player.White, e.Board);
                Dispatcher.Invoke(() => MenuOverlay.Visibility = Visibility.Collapsed);
                DrawBoard(_localGameState.Board); SetCursor(_localGameState.CurrentPlayer);
                this.Title = $"Bạn là quân: {e.MyColor}";
                _gameTimer.Sync(e.WhiteTime, e.BlackTime); _gameTimer.Start(Player.White); UpdateTimerColor();
            };
            _responseHandler.GameUpdated += (s, e) => {
                _localGameState = new GameState(e.CurrentPlayer, e.Board);
                DrawBoard(_localGameState.Board); SetCursor(_localGameState.CurrentPlayer);
                _gameTimer.Sync(e.WhiteTime, e.BlackTime); _gameTimer.Start(e.CurrentPlayer); UpdateTimerColor();
            };
            _responseHandler.ChatReceived += (s, e) => AppendChatMessage(e.Sender, e.Content);
            _responseHandler.WaitingReceived += () => MessageBox.Show("Đang tìm đối thủ...");

            _responseHandler.GameOverFullReceived += (winner, reason) => {
                _isGameOver = true; _gameTimer.Stop();
                Dispatcher.Invoke(() => {
                    MenuOverlay.Visibility = Visibility.Visible;
                    MenuOverlay.ShowGameOver(winner, reason);
                });
            };

            // Sự kiện Menu Overlay
            MenuOverlay.OptionSelected += option => {
                if (option == Option.Restart)
                {
                    _ = _networkClient.SendAsync("REQUEST_RESTART");
                    MenuOverlay.DisableRestartButton();
                }
                else if (option == Option.Exit)
                {
                    _isExiting = true;
                    try { _ = _networkClient.SendAsync("LEAVE_GAME"); } catch { }
                    this.Close();
                }
                else if (option == Option.Analyze)
                {
                    // Gửi yêu cầu phân tích lên Server
                    _ = _networkClient.SendAsync("REQUEST_ANALYSIS");
                }
            };

            _responseHandler.AnalysisDataReceived += OnAnalysisDataReceived; // Đăng ký hàm mở cửa sổ mới

            _responseHandler.AskRestartReceived += () => { Dispatcher.Invoke(() => { var res = MessageBox.Show("Đối thủ muốn chơi lại?", "Tái đấu", MessageBoxButton.YesNo); if (res == MessageBoxResult.Yes) _networkClient.SendAsync("REQUEST_RESTART"); else _networkClient.SendAsync("RESTART_NO"); }); };
            _responseHandler.RestartDeniedReceived += () => Dispatcher.Invoke(() => MessageBox.Show("Đối thủ từ chối."));
            _responseHandler.OpponentLeftReceived += () => { if (_isExiting || _isGameOver) return; Dispatcher.Invoke(() => { MessageBox.Show("Đối thủ đã thoát. Bạn thắng!"); _isExiting = true; this.Close(); }); };
            _gameTimer.Tick += (w, b) => Dispatcher.Invoke(() => { lblWhiteTime.Text = FormatTime(w); lblBlackTime.Text = FormatTime(b); });
        }

        private void StartServerListener()
        {
            Task.Run(() => {
                try { while (!_isExiting && _networkClient.IsConnected) { string msg = _networkClient.WaitForMessage(500); if (_isExiting) break; if (msg == "TIMEOUT") continue; if (msg == null) { if (!_isExiting) Dispatcher.Invoke(() => { if (!_isExiting) { MessageBox.Show("Mất kết nối!"); Close(); } }); break; } Dispatcher.Invoke(() => _responseHandler.ProcessMessage(msg)); } } catch { if (!_isExiting) Dispatcher.Invoke(() => Close()); }
            });
        }

        private void InitializedBoard()
        {
            for (int r = 0; r < 8; r++) for (int c = 0; c < 8; c++)
                {
                    Image i = new Image(); pieceImages[r, c] = i; PieceGrid.Children.Add(i);
                    Rectangle h = new Rectangle(); highlights[r, c] = h; HighlightGrid.Children.Add(h);
                }
        }

        private void LoadBoardImageSafe()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("ChessUI.Assets.Board.png"))
                {
                    if (stream != null)
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit(); bitmap.StreamSource = stream; bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.EndInit();
                        BoardGrid.Background = new ImageBrush(bitmap);
                    }
                }
            }
            catch { }
        }

        private void DrawBoard(Board board) { for (int r = 0; r < 8; r++) for (int c = 0; c < 8; c++) { Position p = (_myColor == Player.Black) ? new Position(7 - r, 7 - c) : new Position(r, c); pieceImages[r, c].Source = Images.GetImage(board[p]); } }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_localGameState == null) return;
            Point p = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(p);
            if (selectedPos == null) OnFromPositionSelected(pos); else OnToPositionSelected(pos);
        }

        private Position ToSquarePosition(Point p) { double s = BoardGrid.ActualWidth / 8; int r = (int)(p.Y / s); int c = (int)(p.X / s); if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; } return new Position(r, c); }
        private void OnFromPositionSelected(Position pos) { if (_localGameState.CurrentPlayer != _myColor) return; var moves = _localGameState.MovesForPiece(pos); if (moves.Any()) { selectedPos = pos; CacheMoves(moves); ShowHighlights(); } }
        private void OnToPositionSelected(Position pos) { selectedPos = null; HideHighlights(); if (moveCache.TryGetValue(pos, out List<Move> moves)) { Move promotionMove = moves.FirstOrDefault(m => m.Type == MoveType.PawnPromotion); if (promotionMove != null) HandlePromotion(promotionMove.FromPos, promotionMove.ToPos); else HandleMove(moves.First()); } }
        private void HandlePromotion(Position from, Position to) { PromotionMenu promMenu = new PromotionMenu(_localGameState.CurrentPlayer); MenuContainer.Content = promMenu; promMenu.PieceSelected += type => { MenuContainer.Content = null; Move finalMove = new PawnPromotion(from, to, type); HandleMove(finalMove); }; }
        private void HandleMove(Move move) { if (_localGameState.CurrentPlayer != _myColor) return; Task.Run(async () => { if (move.Type != MoveType.PawnPromotion && !move.IsLegal(_localGameState.Board)) { Dispatcher.Invoke(() => MessageBox.Show("Nước đi không hợp lệ")); return; } string cmd = $"MOVE|{move.FromPos.Row}|{move.FromPos.Column}|{move.ToPos.Row}|{move.ToPos.Column}"; if (move is PawnPromotion promoMove) cmd += $"|{(int)promoMove.newType}"; await _networkClient.SendAsync(cmd); }); }
        private void CacheMoves(IEnumerable<Move> moves) { moveCache.Clear(); foreach (var m in moves) { if (!moveCache.ContainsKey(m.ToPos)) moveCache[m.ToPos] = new List<Move>(); moveCache[m.ToPos].Add(m); } }
        private void ShowHighlights() { Color c = Color.FromArgb(159, 125, 255, 125); foreach (var p in moveCache.Keys) { int r = p.Row; int col = p.Column; if (_myColor == Player.Black) { r = 7 - r; col = 7 - col; } highlights[r, col].Fill = new SolidColorBrush(c); } }
        private void HideHighlights() { foreach (var p in moveCache.Keys) { int r = p.Row; int c = p.Column; if (_myColor == Player.Black) { r = 7 - r; c = 7 - c; } highlights[r, c].Fill = Brushes.Transparent; } }
        private void SetCursor(Player p) { Cursor = (p == Player.White) ? ChessCursors.WhiteCursor : ChessCursors.BlackCursor; }
        private async void btnSendChat_Click(object s, RoutedEventArgs e) { if (!string.IsNullOrEmpty(txtChatInput.Text)) { await _networkClient.SendAsync($"CHAT|{txtChatInput.Text}"); txtChatInput.Text = ""; } }
        private void txtChatInput_KeyDown(object s, KeyEventArgs e) { if (e.Key == Key.Enter) btnSendChat_Click(s, e); }
        private void AppendChatMessage(string s, string m) { Paragraph p = new Paragraph(); Run r1 = new Run(s + ": ") { FontWeight = FontWeights.Bold, Foreground = (s == "Trắng" || s == "You") ? Brushes.CornflowerBlue : Brushes.Orange }; Run r2 = new Run(m) { Foreground = Brushes.White }; p.Inlines.Add(r1); p.Inlines.Add(r2); txtChatHistory.Document.Blocks.Add(p); txtChatHistory.ScrollToEnd(); }
        private string FormatTime(int s) => TimeSpan.FromSeconds(s).ToString(@"mm\:ss");
        private void UpdateTimerColor() { if (_localGameState == null) return; if (_localGameState.CurrentPlayer == Player.White) { lblWhiteTime.Foreground = Brushes.Red; lblBlackTime.Foreground = Brushes.White; } else { lblWhiteTime.Foreground = Brushes.White; lblBlackTime.Foreground = Brushes.Red; } }
    }
}