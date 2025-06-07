using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Services.Metrics;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class RealtimeMapHub : Hub<IRealtimeMapHub> {

        private readonly ILogger<RealtimeMapHub> _Logger;
        private readonly HubMetric _HubMetric;

        private readonly MapRepository _MapRepository;

        // hubs are transient and re-created every call, store this in cache instead
        private readonly IMemoryCache _Cache;

        public RealtimeMapHub(ILogger<RealtimeMapHub> logger,
            MapRepository worldRepo, IMemoryCache cache,
            HubMetric hubMetric) {

            _Logger = logger;
            _HubMetric = hubMetric;
            _Cache = cache;

            _MapRepository = worldRepo;
        }

        public override Task OnConnectedAsync() {
            _HubMetric.RecordConnect("realtime-map");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            _HubMetric.RecordDisconnect("realtime-map");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Initalize(short worldID, uint zoneID) {
            string groupID = $"RealtimeMap.{worldID}.{zoneID}";

            //_Logger.LogInformation($"{Context.ConnectionId} is subscribing to {groupID}");

            if (_Cache.TryGetValue(Context.ConnectionId, out string? previousGroup) == true) {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousGroup!);
                _Logger.LogTrace($"Removed {Context.ConnectionId} from {previousGroup}");
            }

            _Cache.Set(Context.ConnectionId, groupID, new MemoryCacheEntryOptions() {
                Priority = CacheItemPriority.NeverRemove
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, groupID);
            _Logger.LogTrace($"added {Context.ConnectionId} to {groupID} {Zone.GetName(zoneID)} in {World.GetName(worldID)}");

            PsZone? zone = _MapRepository.GetZone(worldID, zoneID);
            if (zone != null) {
                await Clients.Caller.UpdateMap(zone);
            }
        }

    }
}
