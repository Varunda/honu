using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using DaybreakGames.Census;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Websocket.Client;

namespace watchtower.Services.Offline {

    public class OfflineCensusStreamClient : ICensusStreamClient {

        private List<Func<ReconnectionType, Task>> _OnConnectCallbacks = new List<Func<ReconnectionType, Task>>();
        private List<Func<DisconnectionInfo, Task>> _OnDisconnectCallbacks = new List<Func<DisconnectionInfo, Task>>();
        private List<Func<string, Task>> _OnMessageCallbacks = new List<Func<string, Task>>();

        private CensusStreamClient _Client;

        private readonly ILogger<OfflineCensusStreamClient> _Logger;

        public OfflineCensusStreamClient(ILoggerFactory loggerFactory,
            IOptions<CensusOptions> options) {

            _Logger = loggerFactory.CreateLogger<OfflineCensusStreamClient>();

            _Client = new CensusStreamClient(options, loggerFactory.CreateLogger<CensusStreamClient>());
        }

        public async Task ConnectAsync() {
            foreach (var callback in _OnConnectCallbacks) {
                await callback(ReconnectionType.Initial);
            }
        }

        public async Task DisconnectAsync() {
            Exception ex = new Exception("Offline disconnect exception");
            DisconnectionInfo reason = new DisconnectionInfo(DisconnectionType.Exit, null, "Offline disconnect", "huh", ex);

            foreach (var callback in _OnDisconnectCallbacks) {
               await callback(reason);
            }
        }

        public CensusStreamClient OnConnect(Func<ReconnectionType, Task> onConnect) {
            _OnConnectCallbacks.Add(onConnect);
            return _Client;
        }

        public CensusStreamClient OnDisconnect(Func<DisconnectionInfo, Task> onDisconnect) {
            _OnDisconnectCallbacks.Add(onDisconnect);
            return _Client;
        }

        public CensusStreamClient OnMessage(Func<string, Task> onMessage) {
            _OnMessageCallbacks.Add(onMessage);
            return _Client;
        }

        public Task ReconnectAsync() {
            return Task.FromResult(true);
        }

        public CensusStreamClient SetServiceId(string serviceId) {
            _Client.SetServiceId(serviceId);
            return _Client;
        }

        public CensusStreamClient SetServiceNamespace(string serviceNamespace) {
            _Client.SetServiceNamespace(serviceNamespace);
            return _Client;
        }

        public void Dispose() { }

        public void Subscribe(CensusStreamSubscription subscription) { }

    }

}