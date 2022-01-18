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

        private readonly List<short> _Events = new List<short>() {
            Experience.HEAL, Experience.SQUAD_HEAL,
            Experience.REVIVE, Experience.SQUAD_REVIVE,
            Experience.RESUPPLY, Experience.SQUAD_RESUPPLY,
            Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR,
            Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR,

            Experience.GALAXY_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN, Experience.SQUAD_SPAWN,
            Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS,

            Experience.ASSIST, Experience.SPAWN_ASSIST, Experience.PRIORITY_ASSIST, Experience.HIGH_PRIORITY_ASSIST,

            // I hope they don't add another vehicle cause this list is absurd
            Experience.VKILL_FLASH, Experience.VKILL_GALAXY, Experience.VKILL_LIBERATOR,
            Experience.VKILL_LIGHTNING, Experience.VKILL_MAGRIDER, Experience.VKILL_MOSQUITO,
            Experience.VKILL_SUNDY, Experience.VKILL_PROWLER, Experience.VKILL_REAVER,
            Experience.VKILL_SCYTHE, Experience.VKILL_VANGUARD, Experience.VKILL_HARASSER,
            Experience.VKILL_VALKYRIE, Experience.VKILL_ANT, Experience.VKILL_COLOSSUS,
            Experience.VKILL_JAVELIN, Experience.VKILL_CHIMERA, Experience.VKILL_DERVISH
        };

        private CensusStreamSubscription _Subscription = new CensusStreamSubscription() {
            Worlds = new[] { "all" },
            Characters = new[] { "all" },
        };

        private readonly ILogger<RealtimeMonitor> _Logger;
        private readonly ICensusStreamClient _Stream;
        private readonly IBackgroundTaskQueue _Queue;

        public RealtimeMonitor(ILogger<RealtimeMonitor> logger,
            ICensusStreamClient stream,
            IBackgroundTaskQueue queue) {

            _Subscription.EventNames = _Events.Select(i => $"GainExperience_experience_id_{i}");
            _Subscription.EventNames = _Subscription.EventNames.Append("Death")
                .Append("PlayerLogin").Append("PlayerLogout")
                .Append("BattleRankUp")
                .Append("VehicleDestroy")
                .Append("FacilityControl").Append("PlayerFacilityCapture").Append("PlayerFacilityDefend")
                .Append("ContinentLock").Append("ContinentUnlock").Append("MetagameEvent");

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

        public async Task OnStartAsync(CancellationToken cancel) {
            try {
                await _Stream.ConnectAsync();
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to start RealtimeMonitor");
            }
        }

        public Task OnShutdownAsync(CancellationToken cancel) {
            return _Stream.DisconnectAsync();
        }

        public Task Resubscribe() {
            _Stream.Subscribe(_Subscription);
            return Task.CompletedTask;
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
