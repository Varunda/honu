using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using watchtower.Models.Watchtower;

namespace watchtower.Services.Repositories {

    public class RealtimeNetworkRepository {

        private readonly ILogger<RealtimeNetworkRepository> _Logger;

        private ConcurrentDictionary<short, RealtimeNetwork> _Data = new();

        public RealtimeNetworkRepository(ILogger<RealtimeNetworkRepository> logger) {
            _Logger = logger;
        }

        public RealtimeNetwork? Get(short worldID) {
            lock (_Data) {
                _ = _Data.TryGetValue(worldID, out RealtimeNetwork? network);
                return network;
            }
        }

        public void Set(short worldID, RealtimeNetwork network) {
            lock (_Data) {
                _Data.AddOrUpdate(worldID, network, (_, _) => network);
            }
        }

    }
}
