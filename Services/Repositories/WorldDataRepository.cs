using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    /// <summary>
    /// Repository that holds <see cref="WorldData"/> for a world
    /// </summary>
    public class WorldDataRepository {

        private readonly ILogger<WorldDataRepository> _Logger;

        /// <summary>
        ///     Cache of world data, following the format {WORLD_ID}#{DURATION}, where DURATION is 60/120
        /// </summary>
        private ConcurrentDictionary<string, WorldData> _WorldData = new();

        public WorldDataRepository(ILogger<WorldDataRepository> logger) {
            _Logger = logger;
        }

        /// <summary>
        ///     Get the <see cref="WorldData"/> of a world
        /// </summary>
        /// <param name="worldID">ID of the world to get the world data of</param>
        /// <param name="duration">Duration of the world data</param>
        /// <returns>
        ///     The <see cref="WorldData"/> with <see cref="WorldData.WorldID"/> of <paramref name="worldID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public WorldData? Get(short worldID, int duration) {
            lock (_WorldData) {
                _ = _WorldData.TryGetValue($"{worldID}#{duration}", out WorldData? worldData);
                return worldData;
            }
        }

        /// <summary>
        ///     Set the <see cref="WorldData"/> of a world
        /// </summary>
        /// <param name="worldID">World ID to set the data of</param>
        /// <param name="data">WorldData to be set</param>
        public void Set(short worldID, WorldData data) {
            lock (_WorldData) {
                _WorldData.AddOrUpdate($"{worldID}#{data.Duration}", data, (key, oldValue) => {
                    return data;
                });
            }
        }

    }
}
