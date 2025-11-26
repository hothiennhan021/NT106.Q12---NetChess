using System;
using System.Collections.Concurrent; // <-- SỬ DỤNG CÔNG CỤ MỚI
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class NetworkClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Task _listenTask;
        private bool _isConnected = false;

        // --- GIẢI PHÁP "DỨT ĐIỂM" ---
        // Thay thế Queue/Event cũ bằng BlockingCollection.
        // Đây là một "hàng đợi" an toàn, tự động "block" và "unblock"
        // mà không gây deadlock.
        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>();

        public bool IsConnected => _isConnected;

        public NetworkClient()
        {
            _client = new TcpClient();
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            if (_isConnected) return;

            try
            {
                await _client.ConnectAsync(ipAddress, port);
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = false }; // Tắt AutoFlush
                _isConnected = true;
                StartListening();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi kết nối: " + ex.Message);
                _isConnected = false;
                throw;
            }
        }

        private void StartListening()
        {
            _listenTask = Task.Run(async () =>
            {
                try
                {
                    while (_isConnected)
                    {
                        string message = await _reader.ReadLineAsync();
                        if (message == null)
                        {
                            _isConnected = false;
                            break;
                        }

                        // --- LOG MỚI (CLIENT) ---
                        Console.WriteLine($"[NetworkClient LOG] Đã nhận: {message}");

                        _messageQueue.Add(message);
                    }
                }
                catch (Exception ex)
                {
                    if (_isConnected) // Chỉ báo lỗi nếu đang kết nối
                    {
                        Console.WriteLine("Lỗi khi đọc luồng: " + ex.Message);
                        _isConnected = false;
                    }
                }
                finally
                {
                    // Báo cho hàng đợi biết là không còn tin nào nữa
                    // Điều này sẽ làm Take() ném ra Exception
                    _messageQueue.CompleteAdding();
                }
            });
        }

        // --- HÀM LẤY TIN NHẮN "CHỐNG TREO" ---
        public string WaitForMessage()
        {
            try
            {
                // Hàm .Take() này sẽ TỰ ĐỘNG "ngủ" nếu Queue rỗng
                // và TỰ ĐỘNG "thức dậy" khi có tin.
                // 100% an toàn, không bao giờ "mất" tin nhắn.
                return _messageQueue.Take();
            }
            catch (InvalidOperationException)
            {
                // Bị lỗi này khi .Take() được gọi 
                // sau khi .CompleteAdding() (tức là đã ngắt kết nối)
                _isConnected = false;
                return null;
            }
        }

        public async Task SendAsync(string message)
        {
            if (!_isConnected) return;
            try
            {
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync(); // <-- Luôn Flush
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi tin: " + ex.Message);
                _isConnected = false;
            }
        }

        public void CloseConnection()
        {
            if (!_isConnected) return; // Tránh gọi 2 lần
            _isConnected = false;

            // Đóng các stream sẽ làm ReadLineAsync (trong StartListening)
            // trả về null và thoát luồng một cách an toàn
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();

            // Báo cho Queue dừng lại (để giải phóng _listenerTask của MainWindow)
            _messageQueue.CompleteAdding();
        }
    }
}