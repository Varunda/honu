using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
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
            _Logger.LogInformation($"New connection: {Context.ConnectionId}, count: {++_ConnectionCount}");

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            _Logger.LogInformation($"Hub disconnect: {Context.ConnectionId}, count {--_ConnectionCount}");

            return base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToWorld(short worldID) {
            string connID = Context.ConnectionId;

            // Prevent clients from subscribing to multiple worlds at once
            await Task.WhenAll(
                Groups.RemoveFromGroupAsync(connID, "1"),
                Groups.RemoveFromGroupAsync(connID, "10"),
                Groups.RemoveFromGroupAsync(connID, "13"),
                Groups.RemoveFromGroupAsync(connID, "17"),
                Groups.RemoveFromGroupAsync(connID, "19"),
                Groups.RemoveFromGroupAsync(connID, "40")
            );

            if (World.IsValidWorld(worldID)) {
                await Groups.AddToGroupAsync(connID, worldID.ToString());

                WorldData? data = _WorldDataRepository.Get(worldID);
                if (data != null) {
                    await Clients.Caller.UpdateData(data);
                    _Logger.LogDebug($"Sent previous data made at {data.Timestamp}");
                }
            }
        }

    }
}
