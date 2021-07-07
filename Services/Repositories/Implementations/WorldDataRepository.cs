using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories.Implementations {

    public class WorldDataRepository : IWorldDataRepository {

        private readonly ILogger<WorldDataRepository> _Logger;

        private ConcurrentDictionary<short, WorldData> _WorldData = new ConcurrentDictionary<short, WorldData>();

        public WorldDataRepository(ILogger<WorldDataRepository> logger) {
            _Logger = logger;
        }

        public WorldData? Get(short worldID) {
            lock (_WorldData) {
                _ = _WorldData.TryGetValue(worldID, out WorldData? worldData);
                return worldData;
            }
        }

        public void Set(short worldID, WorldData data) {
            lock (_WorldData) {
                _WorldData.TryAdd(worldID, data);
            }
        }

    }
}
