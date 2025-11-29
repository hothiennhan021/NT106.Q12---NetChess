using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChessClient
{
    public class NetworkClient
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isConnected = false;
        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>();

        public bool IsConnected => _isConnected;

        public NetworkClient() { _client = new TcpClient(); }

        public async Task ConnectAsync(string ip, int port)
        {
            if (_isConnected) return;
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);
            var stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = false };
            _isConnected = true;
            StartListening();
        }

        private void StartListening()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (_isConnected)
                    {
                        string msg = await _reader.ReadLineAsync();
                        if (msg == null) break;
                        _messageQueue.Add(msg);
                    }
                }
                catch { }
                finally { _isConnected = false; _messageQueue.CompleteAdding(); }
            });
        }

        // --- ĐÂY LÀ HÀM QUAN TRỌNG BẠN ĐANG THIẾU ---
        public string WaitForMessage(int timeoutMilliseconds = -1)
        {
            try
            {
                string msg;
                if (timeoutMilliseconds < 0)
                {
                    return _messageQueue.Take(); // Chờ mãi mãi
                }
                else
                {
                    // Chờ có giới hạn (để sửa lỗi Zombie Thread)
                    if (_messageQueue.TryTake(out msg, timeoutMilliseconds))
                    {
                        return msg;
                    }
                    return "TIMEOUT";
                }
            }
            catch { return null; }
        }

        public async Task SendAsync(string msg)
        {
            if (!_isConnected) return;
            try { await _writer.WriteLineAsync(msg); await _writer.FlushAsync(); }
            catch { _isConnected = false; }
        }

        public void CloseConnection()
        {
            _isConnected = false;
            try { _client?.Close(); } catch { }
        }

        public void PurgeMessageQueue()
        {
            while (_messageQueue.TryTake(out _)) { }
        }
    }
}