using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;
using Websocket.Client;

namespace watchtower.Realtime {

    public class RealtimeMonitor : IDisposable, IRealtimeMonitor {

        private readonly List<short> _Events = new List<short>() {
            Experience.HEAL, Experience.SQUAD_HEAL,
            Experience.REVIVE, Experience.SQUAD_REVIVE,
            Experience.RESUPPLY, Experience.SQUAD_RESUPPLY,
            Experience.MAX_REPAIR, Experience.SQUAD_MAX_REPAIR,
            Experience.SHIELD_REPAIR, Experience.SQUAD_SHIELD_REPAIR,
            Experience.VEHICLE_RESUPPLY, Experience.SQUAD_VEHICLE_RESUPPLY,

            Experience.HARDLIGHT_COVER, Experience.DRAW_FIRE_AWARD,

            Experience.GALAXY_SPAWN_BONUS, Experience.GENERIC_NPC_SPAWN, Experience.SQUAD_SPAWN,
            Experience.SQUAD_VEHICLE_SPAWN_BONUS, Experience.SUNDERER_SPAWN_BONUS,

            Experience.ASSIST, Experience.SPAWN_ASSIST, Experience.PRIORITY_ASSIST, Experience.HIGH_PRIORITY_ASSIST,

            // I hope they don't add another vehicle cause this list is absurd
            Experience.VKILL_FLASH, Experience.VKILL_GALAXY, Experience.VKILL_LIBERATOR,
            Experience.VKILL_LIGHTNING, Experience.VKILL_MAGRIDER, Experience.VKILL_MOSQUITO,
            Experience.VKILL_SUNDY, Experience.VKILL_PROWLER, Experience.VKILL_REAVER,
            Experience.VKILL_SCYTHE, Experience.VKILL_VANGUARD, Experience.VKILL_HARASSER,
            Experience.VKILL_VALKYRIE, Experience.VKILL_ANT, Experience.VKILL_COLOSSUS,
            Experience.VKILL_JAVELIN, Experience.VKILL_CHIMERA, Experience.VKILL_DERVISH,

            554
        };

        private CensusStreamSubscription _Subscription = new CensusStreamSubscription() {
            Worlds = new[] { "all" },
            Characters = new[] { "all" },
        };

        private readonly ILogger<RealtimeMonitor> _Logger;
        private readonly ICensusStreamClient _Stream;
        private readonly CensusRealtimeEventQueue _Queue;

        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;

        private readonly System.Timers.Timer _HealthCheckTimer;

        public RealtimeMonitor(ILogger<RealtimeMonitor> logger,
            ICensusStreamClient stream,
            CensusRealtimeEventQueue queue, CensusRealtimeHealthRepository realtimeHealthRepository) {

            _Subscription.EventNames = _Events.Select(i => $"GainExperience_experience_id_{i}");
            foreach (int expId in Experience.VehicleRepairEvents) {
                _Subscription.EventNames = _Subscription.EventNames.Append($"GainExperience_experience_id_{expId}");
            }
            foreach (int expId in Experience.SquadVehicleRepairEvents) {
                _Subscription.EventNames = _Subscription.EventNames.Append($"GainExperience_experience_id_{expId}");
            }
            _Subscription.EventNames = _Subscription.EventNames.Append("Death")
                .Append("PlayerLogin").Append("PlayerLogout")
                .Append("BattleRankUp")
                .Append("VehicleDestroy")
                .Append("FacilityControl").Append("PlayerFacilityCapture").Append("PlayerFacilityDefend")
                .Append("ContinentLock").Append("ContinentUnlock").Append("MetagameEvent");

            _Logger = logger;

            _Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _RealtimeHealthRepository = realtimeHealthRepository;

            _HealthCheckTimer = new System.Timers.Timer();
            _HealthCheckTimer.Interval = 1000;
            _HealthCheckTimer.AutoReset = true;
            _HealthCheckTimer.Elapsed += async (sender, e) => await _HealthCheckTimer_Elapsed(sender, e);

            _Stream.OnConnect(_OnConnectAsync)
                .OnMessage(_OnMessageAsync)
                .OnDisconnect(_OnDisconnectAsync);
        }

        private async Task _HealthCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) {
            try {
                bool needResub = false;

                if (_RealtimeHealthRepository.IsDeathHealthy() == false) {
                    _Logger.LogWarning($"Death is unhealthy, reconnecting");
                    needResub = true;
                }

                if (_RealtimeHealthRepository.IsExpHealthy() == false) {
                    _Logger.LogWarning($"GainExperience is unhealthy, reconnecting");
                    needResub = true;
                }

                if (needResub == true) {
                    await Reconnect();
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"exception in health checker");
            }
        }

        private Task _OnMessageAsync(string msg) {
            if (msg == null) {
                return Task.CompletedTask;
            }

            try {
                JToken token = JToken.Parse(msg);
                _Queue.Queue(token);

                // Events are processed here, as there may be a queue of events. If the health tolerance of a world is 10 seconds,
                //      but the processing is 10 seconds behind, the reconnect will trigger, even tho Honu is still receiving
                //      events, but they are being processed slower than they are coming in
                string? type = token.Value<string?>("type");
                if (type == "serviceMessage") {
                    //_Logger.LogTrace($"serviceMessage");
                    JToken? payload = token.SelectToken("payload");
                    if (payload == null) {
                        return Task.CompletedTask;
                    }

                    //_Logger.LogTrace($"have payload");

                    string? eventName = payload.Value<string?>("event_name");
                    //_Logger.LogTrace($"event_name {eventName}");
                    if (eventName == "Death") {
                        _RealtimeHealthRepository.SetDeath(payload.GetWorldID(), payload.CensusTimestamp("timestamp"));
                    } else if (eventName == "GainExperience") {
                        _RealtimeHealthRepository.SetExp(payload.GetWorldID(), payload.CensusTimestamp("timestamp"));
                    }
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to parse message: {json}", msg);
            }

            return Task.CompletedTask;
        }

        public Task OnStartAsync(CancellationToken cancel) {
            try {
                _ = _Stream.ConnectAsync();
                _HealthCheckTimer.Enabled = true;
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Failed to start RealtimeMonitor");
            }

            return Task.CompletedTask;
        }

        public Task OnShutdownAsync(CancellationToken cancel) {
            _HealthCheckTimer.Enabled = false;
            return _Stream.DisconnectAsync();
        }

        public Task Resubscribe() {
            _Stream.Subscribe(_Subscription);
            return Task.CompletedTask;
        }

        public async Task Reconnect() {
            await _Stream.ReconnectAsync();
        }

        private Task _OnConnectAsync(ReconnectionType type) {
            if (type == ReconnectionType.Initial) {
                _Logger.LogInformation($"Stream connected");
            } else {
                _Logger.LogInformation($"Stream reconnected: {type}");
            }

            _Stream.Subscribe(_Subscription);

            return Task.CompletedTask;
        }

        private Task _OnDisconnectAsync(DisconnectionInfo info) {
            _Logger.LogInformation($"Stream disconnected: {info.Type}");
            return Task.CompletedTask;
        }

        public void Dispose() {
            _HealthCheckTimer.Dispose();
            _Stream?.Dispose();
        }

    }
}
