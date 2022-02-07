using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DaybreakGames.Census.Stream
{
    public class WebSocketWrapper : IDisposable
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Func<string, Task> _onMessage;
        private Action<string> _onDisconnected;

        protected WebSocketWrapper(string uri)
        {
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public WebSocketState State => _ws?.State ?? WebSocketState.Closed;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static WebSocketWrapper Create(string uri)
        {
            return new WebSocketWrapper(uri);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public async Task<WebSocketWrapper> Connect()
        {
            await ConnectAsync();
            return this;
        }

        /// <summary>
        /// Disconnects from the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public async Task<WebSocketWrapper> Disconnect()
        {
            await DisconnectAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <param name="onDisconnect">The Action to call</param>
        /// <returns></returns>
        public WebSocketWrapper OnDisconnect(Action<string> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when a messages has been received.
        /// </summary>
        /// <param name="onMessage">The Action to call.</param>
        /// <returns></returns>
        public WebSocketWrapper OnMessage(Func<string, Task> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public Task SendMessage(string message)
        {
            return SendMessageAsync(message);
        }

        private async Task SendMessageAsync(string message)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
            }
        }

        private async Task ConnectAsync()
        {
            await _ws.ConnectAsync(_uri, _cancellationToken);
            StartListening();
        }

        private async Task DisconnectAsync()
        {
            try
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                _onDisconnected?.Invoke("Application normal closure");
            }
            finally
            {
                _ws?.Dispose();
            }
        }

        private async Task StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                    }
                    else
                    {
                        var count = result.Count;

                        while (!result.EndOfMessage)
                        {
                            if (count >= ReceiveChunkSize)
                            {
                                await DisconnectAsync();
                                return;
                            }

                            result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer, count, ReceiveChunkSize - count), CancellationToken.None);
                            count += result.Count;
                        }

                        var receivedString = Encoding.UTF8.GetString(buffer, 0, count);

                        if (_onMessage != null)
                        {
                            await _onMessage(receivedString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is ObjectDisposedException)
                    return;

                _onDisconnected?.Invoke(ex.Message);
            }
            finally
            {
                _ws?.Dispose();
            }
        }

        private void StartListening()
        {
            Task.Run(() => StartListen());
        }

        public void Dispose()
        {
            _ws?.Dispose();
        }
    }
}
