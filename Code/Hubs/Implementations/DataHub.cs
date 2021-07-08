using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class WorldDataHub : Hub<IWorldDataHub> {

        private readonly ILogger<WorldDataHub> _Logger;

        private static int _ConnectionCount;

        private readonly IWorldDataRepository _WorldDataRepository;

        public WorldDataHub(ILogger<WorldDataHub> logger,
            IWorldDataRepository dataRepo) {

            _Logger = logger;

            _WorldDataRepository = dataRepo ?? throw new ArgumentNullException(nameof(dataRepo));
        }

        public override async Task OnConnectedAsync() {
            WorldData? data = _WorldDataRepository.Get(1);

            if (data != null) {
                await Clients.Caller.UpdateData(data);
                _Logger.LogDebug($"Sent previous data made at {data.Timestamp}");
            }

            _Logger.LogInformation($"New connection: {Context.ConnectionId}, count: {++_ConnectionCount}");

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            _Logger.LogInformation($"Hub disconnect: {Context.ConnectionId}, count {--_ConnectionCount}");

            return base.OnDisconnectedAsync(exception);
        }

    }
}
