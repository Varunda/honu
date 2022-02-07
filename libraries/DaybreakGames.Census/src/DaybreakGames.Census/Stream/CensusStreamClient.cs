using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Websocket.Client;

namespace DaybreakGames.Census.Stream
{
    public class CensusStreamClient : ICensusStreamClient
    {
        private readonly IOptions<CensusOptions> _options;
        private readonly ILogger<CensusStreamClient> _logger;

        private static readonly Func<ClientWebSocket> wsFactory = new Func<ClientWebSocket>(() => {
            return new ClientWebSocket { 
                Options = {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                    RemoteCertificateValidationCallback = (object o, X509Certificate? c, X509Chain? ch, SslPolicyErrors err) => {
                        return true;
                    }
                } 
            };
        });

        private static readonly JsonSerializerSettings sendMessageSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private string _serviceId { get; set; }
        private string _serviceNamespace { get; set; }

        public CensusStreamClient(IOptions<CensusOptions> options, ILogger<CensusStreamClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        private Func<string, Task> _onMessage;
        private Func<DisconnectionInfo, Task> _onDisconnected;
        private Func<ReconnectionType, Task> _onConnect;

        private IWebsocketClient _client;

        public CensusStreamClient OnConnect(Func<ReconnectionType, Task> onConnect)
        {
            _onConnect = onConnect;
            return this;
        }

        public CensusStreamClient OnDisconnect(Func<DisconnectionInfo, Task> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        public CensusStreamClient OnMessage(Func<string, Task> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        public async Task ConnectAsync()
        {

            _client = new WebsocketClient(GetEndpoint(), wsFactory)
            {
                ReconnectTimeout = TimeSpan.FromSeconds(35),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(30)
            };

            _client.DisconnectionHappened.Subscribe(info =>
            {
                _logger.LogWarning(75421, $"Stream disconnected: {info.Type}: {info.Exception}");

                if (_onDisconnected != null)
                {
                    Task.Run(() => _onDisconnected(info));
                }
            });

            _client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    _logger.LogInformation("Starting initial census stream connect");
                }
                else
                {
                    _logger.LogInformation($"Stream reconnection occured: {info.Type}");
                }

                if (_onConnect != null)
                {
                    Task.Run(() => _onConnect(info.Type));
                }
            });

            _client.MessageReceived.Subscribe(msg =>
            {
                if (_onMessage != null)
                {
                    Task.Run(() => _onMessage(msg.Text));
                }
            });

            await _client.Start();
        }

        public void Subscribe(CensusStreamSubscription subscription)
        {
            var sMessage = JsonConvert.SerializeObject(subscription, sendMessageSettings);

            _logger.LogInformation($"Subscribing to census with: {sMessage}");

            _client.Send(sMessage);
        }

        public Task DisconnectAsync()
        {
            _client?.Dispose();
            return Task.CompletedTask;
        }

        public Task ReconnectAsync()
        {
            return _client?.Reconnect();
        }

        private Uri GetEndpoint()
        {
            var ns = _serviceNamespace ?? _options.Value.CensusServiceNamespace ?? Constants.DefaultServiceNamespace;
            var sId = _serviceId ?? _options.Value.CensusServiceId ?? Constants.DefaultServiceId;

            return new Uri($"{Constants.CensusWebsocketEndpoint}?environment={ns}&service-id=s:{sId}");
        }

        public CensusStreamClient SetServiceId(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId));
            }

            _serviceId = serviceId;

            return this;
        }

        public CensusStreamClient SetServiceNamespace(string serviceNamespace)
        {
            if (string.IsNullOrWhiteSpace(serviceNamespace))
            {
                throw new ArgumentNullException(nameof(serviceNamespace));
            }

            _serviceNamespace = serviceNamespace;

            return this;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
