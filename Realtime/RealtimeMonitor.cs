using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services;
using Websocket.Client;

namespace watchtower.Realtime {

    public class RealtimeMonitor : IDisposable, IRealtimeMonitor {

        private readonly List<string> _Events = new List<string>() {
            "4", "51",  // Heals
            "6", "53",  // Revives
            "7", "55",  // Resupplies
            "34", "142" // MAX repairs
        };

        private CensusStreamSubscription _Subscription = new CensusStreamSubscription() {
            Worlds = new[] { "1" },
            Characters = new[] { "all" },
        };

        private readonly ILogger<RealtimeMonitor> _Logger;
        private readonly ICensusStreamClient _Stream;
        private readonly IBackgroundTaskQueue _Queue;

        public RealtimeMonitor(ILogger<RealtimeMonitor> logger,
            ICensusStreamClient stream,
            IBackgroundTaskQueue queue) {

            _Subscription.EventNames = _Events.Select(i => $"GainExperience_experience_id_{i}");
            _Subscription.EventNames = _Subscription.EventNames.Append("Death");

            _Logger = logger;

            _Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _Stream.OnConnect(_OnConnectAsync)
                .OnMessage(_OnMessageAsync)
                .OnDisconnect(_OnDisconnectAsync);
        }

        private Task _OnMessageAsync(string msg) {
            if (msg == null) {
                return Task.CompletedTask;
            }

            try {
                JToken token = JToken.Parse(msg);
                _Queue.Queue(token);
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to parse message: {json}", msg);
            }

            return Task.CompletedTask;
        }

        public Task OnStartAsync(CancellationToken cancel) {
            return _Stream.ConnectAsync();
        }

        public Task OnShutdownAsync(CancellationToken cancel) {
            return _Stream.DisconnectAsync();
        }

        private Task _OnConnectAsync(ReconnectionType type) {
            if (type == ReconnectionType.Initial) {
                _Logger.LogInformation($"Stream connected");
            } else {
                _Logger.LogInformation($"{type}");
            }

            _Stream.Subscribe(_Subscription);

            return Task.CompletedTask;
        }

        private Task _OnDisconnectAsync(DisconnectionInfo info) {
            _Logger.LogInformation($"Stream disconnected: {info.Type}");
            return Task.CompletedTask;
        }

        public void Dispose() {
            _Stream?.Dispose();
        }

    }
}
