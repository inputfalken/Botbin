using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace Botbin.UserTracking.Implementations {
    public class JsonTcpLogger : ILogger, IDisposable {
        private readonly string _address;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private TcpClient _client;
        private readonly ILogger _exceptionLogger;
        private readonly int _port;
        private readonly BlockingCollection<string> _queue;
        private readonly Task _writeTask;
        private bool _disconnected;

        public JsonTcpLogger(string address, int port, ILogger exceptionLogger) {
            _address = address;
            _port = port;
            _exceptionLogger = exceptionLogger;
            _queue = new BlockingCollection<string>();
            _writeTask = Task.Run(Write);
            _cancellationTokenSource = new CancellationTokenSource();
            _client = new TcpClient();
            _disconnected = true;
        }

        public void Dispose() {
            _cancellationTokenSource.Cancel();
            _writeTask.Wait();
            _client?.Dispose();
            _queue?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        public void Log<T>(T item) => _queue.Add(SerializeObject(item));

        private async Task Write() {
            string item = null;
            while (!_cancellationTokenSource.IsCancellationRequested) {
                item = item ?? _queue.Take(_cancellationTokenSource.Token);
                if (item == null) continue;
                try {
                    if (_disconnected) {
                        await _client.ConnectAsync(_address, _port).ConfigureAwait(false);
                        _disconnected = false;
                        _exceptionLogger.Log($"Successfully Connected to '{_address}:{_port}'.");
                    }
                    await Send(item);
                    item = null;
                }
                catch (Exception e) {
                    await HandleException(e);
                }
            }
        }

        private async Task HandleException(Exception e) {
            _exceptionLogger.Log(e);
            await Task.Delay(1000).ConfigureAwait(false);
            // Close calls dispose
            _client.Close();
            _client = new TcpClient();
            _disconnected = true;

        }

        private async Task Send(string message) {
            var buffer = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            await _client.Client.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), SocketFlags.None);
        }
    }
}