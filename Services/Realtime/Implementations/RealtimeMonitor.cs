using DaybreakGames.Census.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;
using Websocket.Client;

namespace watchtower.Realtime {

    public class RealtimeMonitor : IDisposable {

        /// <summary>
        ///     How many seconds minimum between reconnects. Prevents streams that are like 1 second outta sync from causing
        ///     more reconnects than necessary
        /// </summary>
        private const int RECONNECT_MIN_SECONDS = 10;

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
            Experience.VKILL_JAVELIN, Experience.VKILL_CHIMERA, Experience.VKILL_DERVISH
        };

        private readonly ILogger<RealtimeMonitor> _Logger;
        private readonly CensusRealtimeEventQueue _Queue;
        private readonly IServiceProvider _Services;

        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;

        private readonly System.Timers.Timer _HealthCheckTimer;
        private readonly ConcurrentDictionary<string, RealtimeStream> _Streams = new();

        public RealtimeMonitor(ILogger<RealtimeMonitor> logger, IServiceProvider services,
            CensusRealtimeEventQueue queue, CensusRealtimeHealthRepository realtimeHealthRepository) { 

            _Logger = logger;
            _Services = services;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _RealtimeHealthRepository = realtimeHealthRepository;

            _HealthCheckTimer = new System.Timers.Timer();
            _HealthCheckTimer.Interval = 1000;
            _HealthCheckTimer.AutoReset = true;
            _HealthCheckTimer.Elapsed += async (sender, e) => await _HealthCheckTimer_Elapsed(sender, e);
        }

        private async Task _HealthCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e) {
            try {
                List<short> unhealthyWorlds = _RealtimeHealthRepository.GetUnhealthyWorlds();
                if (unhealthyWorlds.Count > 0) {
                    _Logger.LogInformation($"Following worlds are unhealthy: {string.Join(", ", unhealthyWorlds)}");
                }

                foreach (short worldID in unhealthyWorlds) {
                    foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                        // Because Connery would be stream-1*, and Miller is stream-10*, not including the final -
                        //      would cause potentially the wrong world to be found.
                        // For example, if the iteration order happens to be stream-10-Miller, stream-1-Connery,
                        //      then finding stream-1 (meant for Connery), would find stream-10-Miller first, which is wrong
                        // To prevent this, the trailing - is included
                        if (iter.Key.StartsWith($"stream-{worldID}-") == false) {
                            continue;
                        }

                        TimeSpan diff = DateTime.UtcNow - iter.Value.LastConnect;
                        if (diff <= TimeSpan.FromSeconds(RECONNECT_MIN_SECONDS)) {
                            _Logger.LogDebug($"Not reconnecting {iter.Key}: reconnected {iter.Value.LastConnect - DateTime.UtcNow} ago, min of {RECONNECT_MIN_SECONDS}s");
                            break;
                        }

                        await iter.Value.Client.ReconnectAsync();
                        iter.Value.LastConnect = DateTime.UtcNow;
                        _Logger.LogDebug($"reconnected '{iter.Key}' due to bad stream");
                        break;
                    }
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"exception in health checker");
            }
        }

        private Task _OnMessageAsync(string msg) {
            try {
                JToken token = JToken.Parse(msg);
                _Queue.Queue(token);

                // The health of the realtime connection is monitored here, as events are not always guaranteed to be processed in realtime.
                //      For example, if there are 20k events in the task queue, it's likely that processing those events is minutes behind
                //      and if Honu checked the health in the event handler, it would incorrectly see the event is say 3 minutes old,
                //      and reconnect when Honu already has a perfectly good connection.
                // To fix this, the realtime health is updated here, as they're recieved by Honu
                string? type = token.Value<string?>("type");
                if (type == "serviceMessage") {
                    JToken? payload = token.SelectToken("payload");
                    if (payload == null) {
                        return Task.CompletedTask;
                    }

                    string? eventName = payload.Value<string?>("event_name");
                    if (eventName == "Death") {
                        _RealtimeHealthRepository.SetDeath(payload.GetWorldID(), payload.CensusTimestamp("timestamp"));
                    } else if (eventName == "GainExperience") {
                        _RealtimeHealthRepository.SetExp(payload.GetWorldID(), payload.CensusTimestamp("timestamp"));
                    }
                } else if (type == "heartbeat") {
                    //_Logger.LogDebug($"{token}");
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to parse message: {json}", msg);
            }

            return Task.CompletedTask;
        }

        public async Task OnStartAsync(CancellationToken cancel) {
            // Initalized all the worlds to now, useful if a world isn't sending any events on the first connect, we'd like to know that
            foreach (short worldID in World.All) {
                _RealtimeHealthRepository.SetDeath(worldID, DateTime.UtcNow);
                _RealtimeHealthRepository.SetExp(worldID, DateTime.UtcNow);

                RealtimeStream stream = CreateStream($"stream-{worldID}-{World.GetName(worldID)}", "asdf", "ps2");

                CensusStreamSubscription sub = CreateSubscription(worldID);
                stream.Subscriptions.Add(sub);

                await stream.Client.ConnectAsync();
            }

            await ResubscribeAll();

            _HealthCheckTimer.Enabled = true;
        }

        public async Task OnShutdownAsync(CancellationToken cancel) {
            _HealthCheckTimer.Enabled = false;
            foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                await iter.Value.Client.DisconnectAsync();
            }
        }

        /// <summary>
        ///     Resubscribe all streams
        /// </summary>
        public Task ResubscribeAll() {
            foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                foreach (CensusStreamSubscription sub in iter.Value.Subscriptions) {
                    iter.Value.Client.Subscribe(sub);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Reconnect all stream
        /// </summary>
        public async Task ReconnectAll() {
            // TODO: this'll cause problems if a reconnect and an insert happens at the same time
            foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                await iter.Value.Client.ReconnectAsync();
                iter.Value.LastConnect = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Reconnect a specific stream. Case-sensitive
        /// </summary>
        /// <param name="name">Name of the stream to reconnect</param>
        public async Task Reconnect(string name) {
            // TODO: this'll cause problems if a reconnect and an insert happens at the same time
            foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                if (iter.Key != name) {
                    continue;
                }

                await iter.Value.Client.ReconnectAsync();
                iter.Value.LastConnect = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Get the names of all streams
        /// </summary>
        public List<string> GetStreamNames() {
            return _Streams.Keys.ToList();
        }

        private Task _OnDisconnectAsync(DisconnectionInfo info) {
            _Logger.LogInformation($"Stream disconnected: {info.Type}");
            return Task.CompletedTask;
        }

        public void Dispose() {
            GC.SuppressFinalize(this); // idk what this does, but VisualStudio gives me a like hey do this so it's probably smart
            _HealthCheckTimer.Dispose();
            foreach (KeyValuePair<string, RealtimeStream> iter in _Streams) {
                iter.Value.Dispose();
            }
        }

        /// <summary>
        ///     Create a single world subscription
        /// </summary>
        /// <param name="worldID">ID of the world the subscription will be for</param>
        internal CensusStreamSubscription CreateSubscription(short worldID) {
            CensusStreamSubscription sub = new CensusStreamSubscription() {
                Worlds = new List<string>() { $"{worldID}" },
                Characters = new[] { "all" },
                LogicalAndCharactersWithWorlds = true
            };

            sub.EventNames = _Events.Select(i => $"GainExperience_experience_id_{i}");
            foreach (int expId in Experience.VehicleRepairEvents) {
                sub.EventNames = sub.EventNames.Append($"GainExperience_experience_id_{expId}");
            }
            foreach (int expId in Experience.SquadVehicleRepairEvents) {
                sub.EventNames = sub.EventNames.Append($"GainExperience_experience_id_{expId}");
            }
            sub.EventNames = sub.EventNames.Append("Death")
                .Append("PlayerLogin").Append("PlayerLogout")
                .Append("BattleRankUp")
                .Append("VehicleDestroy")
                .Append("FacilityControl").Append("PlayerFacilityCapture").Append("PlayerFacilityDefend")
                .Append("ContinentLock").Append("ContinentUnlock").Append("MetagameEvent");

            return sub;
        }

        /// <summary>
        ///     Create a new named stream, optionally using a specific service ID and platform
        /// </summary>
        /// <param name="name">Name of the stream. Must be unique</param>
        /// <param name="serviceID">Service ID. If left null, the default value will be used</param>
        /// <param name="platform">Platform to use, valid values are ps2|ps2ps4eu|ps2ps4us</param>
        /// <returns>
        ///     The newly created stream, with all callbacks setup. To actually start receiving events on it, you must call .ConnectAsync
        /// </returns>
        /// <exception cref="Exception">If a named stream matching <paramref name="name"/> already exists</exception>
        internal RealtimeStream CreateStream(string name, string? serviceID, string? platform) {
            lock (_Streams) {
                if (_Streams.ContainsKey(name) == true) {
                    throw new Exception($"cannot create another stream with name '{name}'");
                }
            }

            ICensusStreamClient stream = _Services.GetRequiredService<ICensusStreamClient>();
            RealtimeStream wrapper = new RealtimeStream(name, stream);

            _Logger.LogTrace($"Created new stream named '{name}', using platform {platform}");

            if (serviceID != null) { stream.SetServiceId(serviceID); }
            if (platform != null) { stream.SetServiceNamespace(platform); }

            stream.OnConnect((type) => {
                if (type == ReconnectionType.Initial) {
                    _Logger.LogInformation($"Stream '{wrapper.Name}' connected");
                } else {
                    _Logger.LogInformation($"Stream '{wrapper.Name}' reconnected: {type}");
                }

                foreach (CensusStreamSubscription sub in wrapper.Subscriptions) {
                    stream.Subscribe(sub);
                }
                wrapper.LastConnect = DateTime.UtcNow;

                return Task.CompletedTask;
            }).OnMessage(_OnMessageAsync).OnDisconnect(_OnDisconnectAsync);

            lock (_Streams) {
                if (_Streams.TryAdd(name, wrapper) == false) {
                    _Logger.LogError($"failed to add '{name}' to _Streams, TryAdd returned false");
                }
            }

            return wrapper;
        }

    }
}
