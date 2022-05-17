using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using watchtower.Code.Constants;
using watchtower.Models;
using watchtower.Models.Health;

namespace watchtower.Services.Repositories {

    public class CensusRealtimeHealthRepository {

        private readonly ILogger<CensusRealtimeHealthRepository> _Logger;

        private readonly ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Deaths = new ConcurrentDictionary<short, CensusRealtimeHealthEntry>();
        private readonly ConcurrentDictionary<short, CensusRealtimeHealthEntry> _Exp = new ConcurrentDictionary<short, CensusRealtimeHealthEntry>();

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
            lock (_Deaths) {
                _Deaths.AddOrUpdate(worldID, new CensusRealtimeHealthEntry() {
                    LastEvent = timestamp,
                    FailureCount = 0
                }, (key, oldValue) => { 
                    oldValue.FailureCount = 0;
                    oldValue.LastEvent = timestamp;
                    return oldValue;
                });
            }
        }

        /// <summary>
        ///     Set when the most recent exp event for a world was
        /// </summary>
        /// <param name="worldID">ID of the world the exp event occured in</param>
        /// <param name="timestamp">Timestamp of when the exp event occured</param>
        /// <exception cref="ArgumentException">If the world ID was not a valid world</exception>
        public void SetExp(short worldID, DateTime timestamp) {
            lock (_Exp) {
                _Exp.AddOrUpdate(worldID, new CensusRealtimeHealthEntry() {
                    LastEvent = timestamp,
                    FailureCount = 0
                }, (key, oldValue) => { 
                    oldValue.FailureCount = 0;
                    oldValue.LastEvent = timestamp;
                    return oldValue;
                });
            }
        }

        /// <summary>
        ///     Get if the realtime subscription is healthy from the Death events
        /// </summary>
        /// <remarks>
        ///     A world is considered unhealthy if there have been more seconds than the tolerance in the options
        ///     since the last Death event was received
        /// </remarks>
        public bool IsDeathHealthy() {
            //_Logger.LogTrace($"{JToken.FromObject(_HealthTolerances.Value)}");

            bool healthy = true;

            foreach (CensusRealtimeHealthTolerance tolerance in _HealthTolerances.Value.Death) {
                if (tolerance.Tolerance == null) {
                    //_Logger.LogTrace($"Not checking {tolerance.WorldID} cause tolerance is null");
                    continue;
                }

                DateTime? lastUpdated = null;
                lock (_Deaths) {
                    if (_Deaths.TryGetValue(tolerance.WorldID, out CensusRealtimeHealthEntry? l) == true) {
                        lastUpdated = l.LastEvent;
                    }
                }

                if (lastUpdated == null) {
                    //_Logger.LogTrace($"Skipping {tolerance.WorldID} cause it's never been updated yet");
                    continue;
                }

                int timeWithout = (int) Math.Floor((DateTime.UtcNow - lastUpdated.Value).TotalSeconds);

                //_Logger.LogTrace($"World {tolerance.WorldID} went {timeWithout} seconds since a Death event");

                if (timeWithout > tolerance.Tolerance.Value) {
                    int fails;
                    // Assume the reconnection succeeded, bit of a fallback thing
                    lock (_Deaths) {
                        CensusRealtimeHealthEntry entry = _Deaths[tolerance.WorldID];
                        entry.FailureCount = Math.Min(entry.FailureCount, 5);
                        entry.LastEvent = (entry.LastEvent ?? DateTime.UtcNow) + TimeSpan.FromSeconds(10 * entry.FailureCount); // Some back off to be nice
                        fails = ++entry.FailureCount;
                    }
                    _Logger.LogWarning($"World {tolerance.WorldID}/{World.GetName(tolerance.WorldID)} is UNHEALTHY in death events, {timeWithout} seconds old, tolerance {tolerance.Tolerance}, fails = {fails}");
                    healthy = false;
                }
            }

            return healthy;
        }

        /// <summary>
        ///     Get if the realtime subscription is healthy according to the GainExperience events. See <see cref="IsDeathHealthy"/> for more info
        /// </summary>
        public bool IsExpHealthy() {
            bool healthy = true;

            foreach (CensusRealtimeHealthTolerance tolerance in _HealthTolerances.Value.Exp) {
                if (tolerance.Tolerance == null) {
                    continue;
                }

                DateTime? lastUpdated = null;
                lock (_Exp) {
                    if (_Exp.TryGetValue(tolerance.WorldID, out CensusRealtimeHealthEntry? l) == true) {
                        lastUpdated = l.LastEvent;
                    }
                }

                if (lastUpdated == null) {
                    continue;
                }

                int timeWithout = (int) Math.Floor((DateTime.UtcNow - lastUpdated.Value).TotalSeconds);

                if (timeWithout > tolerance.Tolerance.Value) {
                    int fails;
                    lock (_Exp) {
                        CensusRealtimeHealthEntry entry = _Exp[tolerance.WorldID];
                        entry.FailureCount = Math.Min(entry.FailureCount, 5);
                        entry.LastEvent = (entry.LastEvent ?? DateTime.UtcNow) + TimeSpan.FromSeconds(10 * entry.FailureCount); // Some back off to be nice
                        fails = ++entry.FailureCount;
                    }
                    _Logger.LogWarning($"World {tolerance.WorldID}/{World.GetName(tolerance.WorldID)} is UNHEALTHY in exp events, {timeWithout} seconds old, tolerance {tolerance.Tolerance}, fails = {fails}");
                    healthy = false;
                }
            }

            return healthy;
        }

    }
}
