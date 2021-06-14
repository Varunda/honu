using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Services;
using Websocket.Client;

namespace watchtower.Realtime {

    public class RealtimeMonitor : IDisposable, IRealtimeMonitor {

        private readonly List<string> _Events = new List<string>() {
            Experience.HEAL, Experience.SQUAD_HEAL, // 4, 51
            Experience.REVIVE, Experience.SQUAD_REVIVE, // 6, 53
            Experience.RESUPPLY, Experience.SQUAD_RESUPPLY, // 7, 55
            Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR, // 34, 142
            Experience.GALAXY_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN, Experience.SQUAD_SPAWN,
            Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS,
            Experience.ASSIST, Experience.SPAWN_ASSIST, Experience.PRIORITY_ASSIST, Experience.HIGH_PRIORITY_ASSIST
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
            _Subscription.EventNames = _Subscription.EventNames
                .Append("Death")
                .Append("PlayerLogin")
                .Append("PlayerLogout");

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
