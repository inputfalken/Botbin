using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Environment;
using static System.Net.Sockets.SocketFlags;
using static System.Text.Encoding;

namespace Botbin.UserTracking.Implementations {
    internal class JsonLinesTcpLogger : ILogger, IDisposable {
        private readonly string _address;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _exceptionLogger;
        private readonly int _port;
        private readonly BlockingCollection<string> _queue;
        private readonly Task _writeTask;
        private TcpClient _client;
        private bool _connected;

        public JsonLinesTcpLogger(string address, int port, ILogger exceptionLogger) {
            _address = address;
            _port = port;
            _exceptionLogger = exceptionLogger;
            _queue = new BlockingCollection<string>();
            _writeTask = Task.Run(Write);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose() {
            _cancellationTokenSource.Cancel();
            _writeTask.Wait();
            _client?.Dispose();
            _queue?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        public void Log(string log) => _queue.Add(log);

        private async Task Write() {
            string item = null;
            while (!_cancellationTokenSource.IsCancellationRequested) {
                item = item ?? _queue.Take(_cancellationTokenSource.Token);
                if (item == null) continue;
                try {
                    await Connect();
                    await Send(item);
                    item = null;
                }
                catch (Exception e) {
                    await HandleException(e);
                }
            }
        }

        private async Task HandleException(Exception e) {
            _exceptionLogger.Log(e.Message);
            await Task.Delay(1000).ConfigureAwait(false);
            _client?.Dispose();
            _client = null;
            _connected = false;
        }

        private async Task Connect() {
            _client = _client ?? new TcpClient();
            if (!_connected) {
                await _client.ConnectAsync(_address, _port).ConfigureAwait(false);
                _connected = true;
            }
        }

        private async Task Send(string message) =>
            await _client.Client.SendAsync(UTF8.GetBytes(message + NewLine), None);
    }
}