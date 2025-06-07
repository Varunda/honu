using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Services.Metrics;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class WorldOverviewHub : Hub<IWorldOverviewHub> {

        private readonly ILogger<WorldOverviewHub> _Logger;
        private readonly WorldOverviewRepository _WorldOverviewRepository;
        private readonly HubMetric _HubMetric;

        public WorldOverviewHub(ILogger<WorldOverviewHub> logger,
            WorldOverviewRepository worldRepo, HubMetric hubMetric) {

            _Logger = logger;
            _WorldOverviewRepository = worldRepo;
            _HubMetric = hubMetric;
        }

        public override async Task OnConnectedAsync() {
            _HubMetric.RecordConnect("world-overview");
            //_Logger.LogInformation($"New connection: {Context.ConnectionId}, count: {++_ConnectionCount}");
            await base.OnConnectedAsync();
            await Clients.Caller.UpdateData(_WorldOverviewRepository.Build());
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            _HubMetric.RecordDisconnect("world-overview");
            return base.OnDisconnectedAsync(exception);
        }

    }
}
