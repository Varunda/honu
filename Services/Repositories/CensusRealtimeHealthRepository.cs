using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Health;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class CensusRealtimeHealthRepository {

        private readonly ILogger<CensusRealtimeHealthRepository> _Logger;

        private readonly BadHealthRepository _BadHealthRepository;
        private readonly RealtimeReconnectDbStore _ReconnectDb;

        private ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Deaths = new();
        private ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Exp = new();

        private readonly IOptions<CensusRealtimeHealthOptions> _HealthTolerances;

        public CensusRealtimeHealthRepository(ILogger<CensusRealtimeHealthRepository> logger,
            IOptions<CensusRealtimeHealthOptions> healthTolerances, BadHealthRepository badHealthRepository,
            RealtimeReconnectDbStore reconnectDb) {

            _Logger = logger;

            _HealthTolerances = healthTolerances;
            _BadHealthRepository = badHealthRepository;
            _ReconnectDb = reconnectDb;
        }

        /// <summary>
        ///     Set when the most recent death event for a world was
        /// </summary>
        /// <param name="worldID">ID of the world the death event occured in</param>
        /// <param name="timestamp">Timestamp of when the death event occured</param>
        /// <param name="data">json token, used for out of order detection</param>
        /// <exception cref="ArgumentException">If the world ID was not a valid world</exception>
        public bool SetDeath(short worldID, DateTime timestamp, JToken? data) {
            return SetMap(ref _Deaths, worldID, timestamp, "death", data);
        }

        /// <summary>
        ///     Set when the most recent exp event for a world was
        /// </summary>
        /// <param name="worldID">ID of the world the exp event occured in</param>
        /// <param name="timestamp">Timestamp of when the exp event occured</param>
        /// <param name="data">json token, used for out of order detection</param>
        /// <exception cref="ArgumentException">If the world ID was not a valid world</exception>
        public bool SetExp(short worldID, DateTime timestamp, JToken? data) {
            return SetMap(ref _Exp, worldID, timestamp, "exp", data);
        }

        /// <summary>
        ///     Get if the realtime subscription is healthy from the Death events
        /// </summary>
        /// <remarks>
        ///     A world is considered unhealthy if there have been more seconds than the tolerance in the options
        ///     since the last Death event was received
        /// </remarks>
        public bool IsDeathHealthy() {
            return IsDictHealth(ref _Deaths, "death");
        }

        /// <summary>
        ///     Get if the realtime subscription is healthy according to the GainExperience events. See <see cref="IsDeathHealthy"/> for more info
        /// </summary>
        public bool IsExpHealthy() {
            return IsDictHealth(ref _Exp, "exp");
        }

        /// <summary>
        ///     Get a list of world IDs that currently do not have a healthy stream
        /// </summary>
        public List<short> GetUnhealthyWorlds() {
            List<short> unhealthyWorlds = GetUnhealthyDeath();

            if (_HealthTolerances.Value.RequireFailureOnBoth == true) {
                unhealthyWorlds = unhealthyWorlds.Intersect(GetUnhealthyExp()).ToList();
            } else {
                unhealthyWorlds = unhealthyWorlds.Union(GetUnhealthyExp()).ToList();
            }

            return unhealthyWorlds;
        }

        /// <summary>
        ///     Get all worlds that currently have an unhealthy Death stream
        /// </summary>
        /// <remarks>
        ///     A world is considered unhealthy if there have been more seconds than the tolerance in the options
        ///     since the last Death event was received
        /// </remarks>
        /// <returns></returns>
        public List<short> GetUnhealthyDeath() {
            return GetUnhealthyDict("death", ref _Deaths, _HealthTolerances.Value.Death);
        }

        public List<short> GetUnhealthyExp() {
            return GetUnhealthyDict("exp", ref _Exp, _HealthTolerances.Value.Exp);
        }

        public List<CensusRealtimeHealthEntry> GetDeathHealth() {
            return _Deaths.Values.ToList();
        }

        public List<CensusRealtimeHealthEntry> GetExpHealth() {
            return _Exp.Values.ToList();
        }

        /// <summary>
        ///     Update one of the event health entries
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="worldID"></param>
        /// <param name="timestamp"></param>
        /// <param name="what"></param>
        /// <param name="data"></param>
        private bool SetMap(ref ConcurrentDictionary<short, CensusRealtimeHealthEntry> dict, short worldID, DateTime timestamp, string what, JToken? data) {

            bool ret = false;

            lock (dict) {
                dict.AddOrUpdate(worldID, new CensusRealtimeHealthEntry() {
                    WorldID = worldID,
                    FirstEvent = timestamp,
                    LastEvent = timestamp,
                    FailureCount = 0,
                    EventCount = 0
                }, (key, oldValue) => { 
                    if (oldValue.FailureCount > 0) {
                        if (oldValue.LastEvent != null) {
                            int timeWithout = Math.Max(0, (int) Math.Floor((DateTime.UtcNow - oldValue.LastEvent.Value).TotalSeconds)); 

                            RealtimeReconnectEntry entry = new RealtimeReconnectEntry() {
                                WorldID = oldValue.WorldID,
                                Duration = timeWithout,
                                EventCount = oldValue.EventCount,
                                FailedCount = oldValue.FailureCount,
                                StreamType = what,
                                Timestamp = DateTime.UtcNow
                            };

                            _BadHealthRepository.Insert(new BadHealthEntry() {
                                When = timestamp,
                                What = $"World {worldID}/{World.GetName(worldID)}'s {what} stream was {timeWithout}s without an event, failed {oldValue.FailureCount} times before reconnect, had {entry.EventCount} events on the stream"
                            });

                            new Thread(async () => {
                                try {
                                    await _ReconnectDb.Insert(entry, CancellationToken.None);
                                } catch (Exception ex) {
                                    _Logger.LogError(ex, $"error in background thread inserting the {nameof(RealtimeReconnectEntry)}");
                                }
                            }).Start();
                        }

                        oldValue.EventCount = 0;
                        oldValue.FirstEvent = timestamp;

                        _Logger.LogDebug($"World {oldValue.WorldID} got an event, resetting failure count");
                    }

                    // if the timestamp would be rolled back more than 2 seconds, record this
                    if (oldValue.LastEvent != null && ((oldValue.LastEvent - timestamp) >= TimeSpan.FromSeconds(1))) {
                        TimeSpan diff = oldValue.LastEvent.Value - timestamp;
                        _Logger.LogInformation($"out of order timestamp found! [worldID={worldID}] [what={what}] [diff={diff}] [timestamp={timestamp:u}] [LastEvent={oldValue.LastEvent:u}]");

                        if (oldValue.LastEventData != null) {
                            _Logger.LogDebug($"out of order event data [LastEventData={oldValue.LastEventData.ToString(Newtonsoft.Json.Formatting.None)}] [new event={data?.ToString(Newtonsoft.Json.Formatting.None)}]");
                        }

                        ret = true;
                    }

                    oldValue.FailureCount = 0;
                    oldValue.LastEvent = timestamp;
                    oldValue.LastEventData = data;
                    ++oldValue.EventCount;
                    return oldValue;
                });
            }

            return ret;
        }

        private List<short> GetUnhealthyDict(string type, ref ConcurrentDictionary<short, CensusRealtimeHealthEntry> dict,
            List<CensusRealtimeHealthTolerance> tolerances) {

            List<short> badWorlds = new List<short>();

            foreach (CensusRealtimeHealthTolerance tolerance in tolerances) {
                if (tolerance.Tolerance == null) {
                    //_Logger.LogTrace($"Not checking {tolerance.WorldID} cause tolerance is null");
                    continue;
                }

                if (dict.TryGetValue(tolerance.WorldID, out CensusRealtimeHealthEntry? entry) == false) {
                    continue;
                }

                if (entry.LastEvent == null) {
                    continue;
                }

                // Backoff based on the failure count. The more times Honu has failed to get a value, back off more and more
                int backoff = Math.Min(10, entry.FailureCount + 1);
                int threshold = (tolerance.Tolerance.Value * (entry.FailureCount + 1)) + (10 * Math.Min(3, entry.FailureCount));

                int playerCount = CharacterStore.Get().GetWorldCount(tolerance.WorldID);
                if (playerCount < 100) {
                    threshold *= 4;
                } else if (playerCount < 200) {
                    threshold *= 2;
                }

                int timeWithout = Math.Max(0, (int) Math.Floor((DateTime.UtcNow - entry.LastEvent.Value).TotalSeconds)); 

                //_Logger.LogTrace($"World {tolerance.WorldID} has gone {timeWithout} seconds without a {type} event, theshold is {threshold} seconds (has {entry.FailureCount} failures)");

                if (timeWithout > threshold) {
                    ++entry.FailureCount;

                    dict[tolerance.WorldID] = entry;

                    badWorlds.Add(tolerance.WorldID);

                    _Logger.LogWarning($"World {tolerance.WorldID}/{World.GetName(tolerance.WorldID)} is UNHEALTHY in {type} events, "
                        + $"{timeWithout} seconds old, threshold {threshold}, backoff {backoff}, fails {entry.FailureCount}, players {playerCount}");
                }
            }

            return badWorlds;
        }

        private bool IsDictHealth(ref ConcurrentDictionary<short, CensusRealtimeHealthEntry> dict, string type) {
            //_Logger.LogTrace($"{JToken.FromObject(_HealthTolerances.Value)}");

            bool healthy = true;

            foreach (CensusRealtimeHealthTolerance tolerance in _HealthTolerances.Value.Death) {
                if (tolerance.Tolerance == null) {
                    //_Logger.LogTrace($"Not checking {tolerance.WorldID} cause tolerance is null");
                    continue;
                }

                if (dict.TryGetValue(tolerance.WorldID, out CensusRealtimeHealthEntry? entry) == false) {
                    continue;
                }

                if (entry.LastEvent == null) {
                    continue;
                }

                // Backoff based on the failure count. The more times Honu has failed to get a value, back off more and more
                int backoff = Math.Min(10, entry.FailureCount + 1);
                int threshold = (tolerance.Tolerance.Value * (entry.FailureCount + 1)) + (10 * Math.Min(3, entry.FailureCount));

                int playerCount = CharacterStore.Get().GetWorldCount(tolerance.WorldID);
                if (playerCount < 100) {
                    threshold *= 4;
                } else if (playerCount < 200) {
                    threshold *= 2;
                }

                int timeWithout = Math.Max(0, (int) Math.Floor((DateTime.UtcNow - entry.LastEvent.Value).TotalSeconds)); 

                //_Logger.LogTrace($"World {tolerance.WorldID} has gone {timeWithout} seconds without a {type} event, theshold is {threshold} seconds (has {entry.FailureCount} failures)");

                if (timeWithout > threshold) {
                    ++entry.FailureCount;

                    dict[tolerance.WorldID] = entry;
                    healthy = false;

                    _Logger.LogWarning($"World {tolerance.WorldID}/{World.GetName(tolerance.WorldID)} is UNHEALTHY in {type} events, {timeWithout} seconds old, tolerance {threshold}, backoff {backoff}, fails {entry.FailureCount}");
                }
            }

            return healthy;
        }

    }
}
