using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Health;

namespace watchtower.Services.Repositories {

    public class CensusRealtimeHealthRepository {

        private readonly ILogger<CensusRealtimeHealthRepository> _Logger;

        private ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Deaths = new ConcurrentDictionary<short, CensusRealtimeHealthEntry>();
        private ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Exp = new ConcurrentDictionary<short, CensusRealtimeHealthEntry>();

        private readonly IOptions<CensusRealtimeHealthOptions> _HealthTolerances;

        public CensusRealtimeHealthRepository(ILogger<CensusRealtimeHealthRepository> logger,
            IOptions<CensusRealtimeHealthOptions> healthTolerances) {

            _Logger = logger;
            _HealthTolerances = healthTolerances;
        }

        /// <summary>
        ///     Set when the most recent death event for a world was
        /// </summary>
        /// <param name="worldID">ID of the world the death event occured in</param>
        /// <param name="timestamp">Timestamp of when the death event occured</param>
        /// <exception cref="ArgumentException">If the world ID was not a valid world</exception>
        public void SetDeath(short worldID, DateTime timestamp) {
            SetMap(ref _Deaths, worldID, timestamp);
        }

        /// <summary>
        ///     Set when the most recent exp event for a world was
        /// </summary>
        /// <param name="worldID">ID of the world the exp event occured in</param>
        /// <param name="timestamp">Timestamp of when the exp event occured</param>
        /// <exception cref="ArgumentException">If the world ID was not a valid world</exception>
        public void SetExp(short worldID, DateTime timestamp) {
            SetMap(ref _Exp, worldID, timestamp);
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
        private void SetMap(ref ConcurrentDictionary<short, CensusRealtimeHealthEntry> dict, short worldID, DateTime timestamp) {
            lock (dict) {
                dict.AddOrUpdate(worldID, new CensusRealtimeHealthEntry() {
                    WorldID = worldID,
                    LastEvent = timestamp,
                    FailureCount = 0
                }, (key, oldValue) => { 
                    if (oldValue.FailureCount > 0) {
                        _Logger.LogDebug($"World {oldValue.WorldID} got an event, resetting failure count");
                    }

                    oldValue.FailureCount = 0;
                    oldValue.LastEvent = timestamp;
                    return oldValue;
                });
            }
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
                int threshold = tolerance.Tolerance.Value * Math.Min(10, entry.FailureCount + 1);

                int playerCount = CharacterStore.Get().GetWorldCount(tolerance.WorldID);
                if (playerCount < 200) {
                    threshold *= 2;
                }

                int timeWithout = Math.Max(0, (int) Math.Floor((DateTime.UtcNow - entry.LastEvent.Value).TotalSeconds)); 

                //_Logger.LogTrace($"World {tolerance.WorldID} has gone {timeWithout} seconds without a {type} event, theshold is {threshold} seconds (has {entry.FailureCount} failures)");

                if (timeWithout > threshold) {
                    ++entry.FailureCount;

                    dict[tolerance.WorldID] = entry;
                    healthy = false;

                    _Logger.LogWarning($"World {tolerance.WorldID}/{World.GetName(tolerance.WorldID)} is UNHEALTHY in {type} events, {timeWithout} seconds old, tolerance {tolerance.Tolerance}, fails {entry.FailureCount}");
                }
            }

            return healthy;
        }

    }
}
