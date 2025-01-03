using DaybreakGames.Census.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Services.Metrics;
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

        public static bool UseNss = false;

        private readonly ILogger<RealtimeMonitor> _Logger;
        private readonly CensusRealtimeEventQueue _Queue;
        private readonly IServiceProvider _Services;

        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;

        private readonly RealtimeStreamMetric _Metrics;

        private readonly System.Timers.Timer _HealthCheckTimer;
        private readonly ConcurrentDictionary<string, RealtimeStream> _Streams = new();

        private readonly HashSet<string> _ReconnectingStream = new();

        public RealtimeMonitor(ILogger<RealtimeMonitor> logger, IServiceProvider services,
            CensusRealtimeEventQueue queue, CensusRealtimeHealthRepository realtimeHealthRepository,
            RealtimeStreamMetric metrics) {

            _Logger = logger;
            _Services = services;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _RealtimeHealthRepository = realtimeHealthRepository;

            _HealthCheckTimer = new System.Timers.Timer();
            _HealthCheckTimer.Interval = 1000;
            _HealthCheckTimer.AutoReset = true;
            _HealthCheckTimer.Elapsed += async (sender, e) => await _HealthCheckTimer_Elapsed(sender, e);

            _Metrics = metrics;
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

                        if (_ReconnectingStream.Contains(iter.Key)) {
                            _Logger.LogDebug($"not reconnecting stream as it is already in progress [name={iter.Value.Name}]");
                            continue;
                        }

                        TimeSpan diff = DateTime.UtcNow - iter.Value.LastConnect;
                        if (diff <= TimeSpan.FromSeconds(RECONNECT_MIN_SECONDS)) {
                            _Logger.LogDebug($"Not reconnecting {iter.Key}: reconnected {iter.Value.LastConnect - DateTime.UtcNow} ago, min of {RECONNECT_MIN_SECONDS}s");
                            break;
                        }

                        _Logger.LogDebug($"reconnecting stream [name={iter.Value.Name}]");
                        lock (_ReconnectingStream) {
                            _ReconnectingStream.Add(iter.Key);
                        }
                        await iter.Value.Client.ReconnectAsync();
                        lock (_ReconnectingStream) {
                            _ReconnectingStream.Remove(iter.Key);
                        }
                        iter.Value.LastConnect = DateTime.UtcNow;
                        _Logger.LogDebug($"stream reconnected [name={iter.Value.Name}]");
                        _Metrics.RecordReconnect(worldID);
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

                //_Logger.LogDebug($"{token}");

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
                        _RealtimeHealthRepository.SetDeath(payload.GetWorldID(), payload.CensusTimestamp("timestamp"), payload);
                    } else if (eventName == "GainExperience") {
                        _RealtimeHealthRepository.SetExp(payload.GetWorldID(), payload.CensusTimestamp("timestamp"), payload);
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
            await CreateAllStreams(cancel);
        }

        public async Task CreateAllStreams(CancellationToken cancel = default) {

            // clear previous streams
            _Logger.LogInformation($"clearing previous streams [stream count={_Streams.Count}]");
            foreach (KeyValuePair<string, RealtimeStream> s in _Streams) {
                s.Value.Dispose();
            }
            _Streams.Clear();

            List<short> worlds = new();
            worlds.AddRange(World.PcStreams);
            worlds.AddRange(World.Ps4UsStreams);
            worlds.AddRange(World.Ps4EuStreams);

            foreach (short worldID in worlds) {
                try {
                    new Thread(async () => {
                        _RealtimeHealthRepository.SetDeath(worldID, DateTime.UtcNow, null);
                        _RealtimeHealthRepository.SetExp(worldID, DateTime.UtcNow, null);

                        CensusEnvironment? env = CensusEnvironmentHelper.FromWorldID(worldID);
                        if (env == null) {
                            _Logger.LogError($"Unknown {nameof(CensusEnvironment)} from world ID {worldID}, defaulting to PC for creating realtime stream");
                            env = CensusEnvironment.PC;
                        }

                        RealtimeStream stream = CreateStream($"stream-{worldID}-{World.GetName(worldID)}", "asdf", env.Value);

                        CensusStreamSubscription sub = CreateSubscription(worldID);
                        stream.Subscriptions.Add(sub);

                        try {
                            _Logger.LogInformation($"stream connecting [name={stream.Name}]");
                            await stream.Client.ConnectAsync();
                            _Logger.LogDebug($"stream connected [name={stream.Name}]");
                        } catch (Exception ex) {
                            _Logger.LogError($"failed to create stream {stream.Name}: {ex.Message}", ex);
                        }
                    }).Start();
                } catch (Exception ex) {
                    _Logger.LogError($"failure in background thread to create stream for {worldID}: {ex.Message}", ex);
                }
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
                Worlds = [$"{worldID}"],
                Characters = ["all"],
                LogicalAndCharactersWithWorlds = true
            };

            sub.EventNames = ["GainExperience", "AchievementEarned", "ItemAdded"];

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
        /// <param name="environment">Platform to use, valid values are ps2|ps2ps4eu|ps2ps4us</param>
        /// <returns>
        ///     The newly created stream, with all callbacks setup. To actually start receiving events on it, you must call .ConnectAsync
        /// </returns>
        /// <exception cref="Exception">If a named stream matching <paramref name="name"/> already exists</exception>
        internal RealtimeStream CreateStream(string name, string? serviceID, CensusEnvironment environment) {
            lock (_Streams) {
                if (_Streams.ContainsKey(name) == true) {
                    throw new Exception($"cannot create another stream with name '{name}'");
                }
            }

            ICensusStreamClient stream = _Services.GetRequiredService<ICensusStreamClient>();
            if (UseNss == true) {
                stream.SetEndpoint("wss://push.nanite-systems.net/streaming");
            }
            RealtimeStream wrapper = new RealtimeStream(name, stream);

            _Logger.LogDebug($"created realtime stream [name={name}] [environment={environment}] [useNss={UseNss}]");

            if (serviceID != null) { stream.SetServiceId(serviceID); }
            stream.SetServiceNamespace(CensusEnvironmentHelper.ToNamespace(environment));

            stream.OnConnect((type) => {
                if (type == ReconnectionType.Initial) {
                    _Logger.LogInformation($"stream connected [name={wrapper.Name}]");
                } else {
                    _Logger.LogInformation($"stream reconnected [name={wrapper.Name}] [type={type}]");
                }

                foreach (CensusStreamSubscription sub in wrapper.Subscriptions) {
                    stream.Subscribe(sub);
                }
                wrapper.LastConnect = DateTime.UtcNow;

                lock (_ReconnectingStream) {
                    if (_ReconnectingStream.Contains(name)) {
                        _Logger.LogWarning($"the failsafe reconnecting streams logic was hit! this is not supposed to happen! [name={name}]");
                        _ReconnectingStream.Remove(name);
                    }
                }

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
