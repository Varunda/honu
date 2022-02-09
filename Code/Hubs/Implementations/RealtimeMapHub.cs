using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class RealtimeMapHub : Hub<IRealtimeMapHub> {

        private readonly ILogger<RealtimeMapHub> _Logger;
        private readonly MapRepository _MapRepository;

        private readonly Dictionary<string, string> _GroupMembership = new Dictionary<string, string>(); // <connection id, group>

        public RealtimeMapHub(ILogger<RealtimeMapHub> logger,
            MapRepository worldRepo) {

            _Logger = logger;
            _MapRepository = worldRepo;
        }

        public async Task Initalize(short worldID, uint zoneID) {
            string groupID = $"RealtimeMap.{worldID}.{zoneID}";

            _Logger.LogInformation($"{Context.ConnectionId} is subscribing to {groupID}");

            if (_GroupMembership.TryGetValue(Context.ConnectionId, out string? previousGroup) == true) {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousGroup!);
                _Logger.LogTrace($"Removed {Context.ConnectionId} from {previousGroup}");
            }

            _GroupMembership[Context.ConnectionId] = groupID;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupID);

            PsZone? zone = _MapRepository.GetZone(worldID, zoneID);
            if (zone != null) {
                await Clients.Caller.UpdateMap(zone);
            }
        }

    }
}
