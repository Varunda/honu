using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Watchtower;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class RealtimeNetworkHub : Hub<IRealtimeNetworkHub> {

        private readonly ILogger<RealtimeNetworkHub> _Logger;
        private readonly RealtimeNetworkRepository _NetworkRepository;

        private readonly Dictionary<string, string> _GroupMembership = new Dictionary<string, string>(); // <connection id, group>

        public RealtimeNetworkHub(ILogger<RealtimeNetworkHub> logger,
            RealtimeNetworkRepository networkRepository) {

            _Logger = logger;
            _NetworkRepository = networkRepository;
        }

        public async Task Initalize(short worldID) {
            try {
                string groupID = $"RealtimeNetwork.{worldID}";

                _Logger.LogInformation($"{Context.ConnectionId} is subscribing to {groupID}");

                if (_GroupMembership.TryGetValue(Context.ConnectionId, out string? previousGroup) == true) {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousGroup!);
                    _Logger.LogTrace($"Removed {Context.ConnectionId} from {previousGroup}");
                }

                _GroupMembership[Context.ConnectionId] = groupID;
                await Groups.AddToGroupAsync(Context.ConnectionId, groupID);

                RealtimeNetwork? network = _NetworkRepository.Get(worldID);
                if (network != null) {
                    await Clients.Caller.UpdateNetwork(network);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to initalize");
            }
        }

    }
}
