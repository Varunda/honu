using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class RealtimeMapStateRepository {

        private readonly ILogger<RealtimeMapStateRepository> _Logger;
        private readonly RealtimeMapStateCollection _Census;
        private readonly RealtimeMapStateDbStore _Db;

        private readonly Dictionary<string, RealtimeMapState> _PreviousState = new();

        private readonly IMemoryCache _Cache;

        public RealtimeMapStateRepository(ILogger<RealtimeMapStateRepository> logger, IMemoryCache cache,
            RealtimeMapStateDbStore db, RealtimeMapStateCollection census) {

            _Logger = logger;
            _Cache = cache;

            _Db = db;
            _Census = census;
        }

        /// <summary>
        ///     Update the database with entries that have changed since the last run
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public async Task Update(CancellationToken cancel) {
            long sameCount = 0;
            long changedCount = 0;

            _Logger.LogDebug($"getting map state and saving to db");
            Stopwatch timer = Stopwatch.StartNew();

            List<RealtimeMapState> states = await _Census.GetAll(cancel);
            long censusMs = timer.ElapsedMilliseconds; timer.Restart();

            foreach (RealtimeMapState state in states) {
                string key = $"{state.WorldID}.{state.ZoneID}.{state.RegionID}";
                if (_PreviousState.TryGetValue(key, out RealtimeMapState? previousState) == true) {
                    if (previousState != null && previousState == state) {
                        ++sameCount;
                        continue;
                    }
                }

                ++changedCount;
                await _Db.Insert(state, cancel);

                lock (_PreviousState) {
                    _PreviousState[key] = state;
                }
            }
            long dbMs = timer.ElapsedMilliseconds;

            _Logger.LogInformation($"saved realtime map info in {censusMs + dbMs}ms, updated {changedCount} entries. "
                + $"[Census={censusMs}ms] [DB={dbMs}ms] [Same count={sameCount}] [Changed count={changedCount}]");
        }

        /// <summary>
        ///     Get the <see cref="RealtimeMapState"/> for a specific world and region between a period of time
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="regionID">ID of the region (not facility!)</param>
        /// <param name="start">When to start the grab period</param>
        /// <param name="end">When to end the grab period</param>
        /// <returns>
        ///     A list of <see cref="RealtimeMapState"/> with <see cref="RealtimeMapState.WorldID"/> of <paramref name="worldID"/>,
        ///     <see cref="RealtimeMapState.RegionID"/> of <paramref name="regionID"/>, 
        ///     and <see cref="RealtimeMapState.Timestamp"/> between <paramref name="start"/> and <paramref name="end"/>
        /// </returns>
        public Task<List<RealtimeMapState>> GetHistoricalByWorldAndRegion(short worldID, int regionID, DateTime start, DateTime end) {
            return _Db.GetHistoricalByWorldAndRegion(worldID, regionID, start, end);
        }

        /// <summary>
        ///     Get a list of the current <see cref="RealtimeMapState"/> for a specific world
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <returns>
        ///     A list of <see cref="RealtimeMapState"/>s from <see cref="_PreviousState"/> with a
        ///     <see cref="RealtimeMapState.WorldID"/> of <paramref name="worldID"/>
        /// </returns>
        public Task<List<RealtimeMapState>> GetByWorld(short worldID) {
            lock (_PreviousState) {
                List<RealtimeMapState> map = _PreviousState.Values.Where(iter => iter.WorldID == worldID).ToList();

                return Task.FromResult(map);
            }
        }

        /// <summary>
        ///     Get a list of the most recent <see cref="RealtimeMapState"/> for a specific world and zone
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="zoneID">ID of the zone</param>
        /// <returns>
        ///     A list of <see cref="RealtimeMapState"/> from <see cref="_PreviousState"/> with a 
        ///     <see cref="RealtimeMapState.WorldID"/> of <paramref name="worldID"/> and 
        ///     a <see cref="RealtimeMapState.ZoneID"/> of <paramref name="zoneID"/>
        /// </returns>
        public Task<List<RealtimeMapState>> GetByWorldAndZone(short worldID, uint zoneID) {
            lock (_PreviousState) {
                List<RealtimeMapState> map = _PreviousState.Values.Where(iter => iter.WorldID == worldID && iter.ZoneID == zoneID).ToList();

                return Task.FromResult(map);
            }
        }

    }
}
