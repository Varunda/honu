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

        private readonly IWorldDataRepository _WorldDataRepository;

        public WorldDataHub(ILogger<WorldDataHub> logger,
            IWorldDataRepository dataRepo) {

            _Logger = logger;

            _WorldDataRepository = dataRepo ?? throw new ArgumentNullException(nameof(dataRepo));
        }

        public override async Task OnConnectedAsync() {
            //_Logger.LogInformation($"New connection: {Context.ConnectionId}, count: {++_ConnectionCount}");

            lock (ConnectionStore.Get().Connections) {
                ConnectionStore.Get().Connections.GetOrAdd(Context.ConnectionId, new TrackedConnection() {
                    ConnectionId = Context.ConnectionId
                });
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            //_Logger.LogInformation($"Hub disconnect: {Context.ConnectionId}, count {--_ConnectionCount}");

            lock (ConnectionStore.Get().Connections) {
                ConnectionStore.Get().Connections.TryRemove(Context.ConnectionId, out _);
            }

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

                lock (ConnectionStore.Get().Connections) {
                    TrackedConnection conn = ConnectionStore.Get().Connections.GetOrAdd(connID, new TrackedConnection() {
                        ConnectionId = connID,
                        WorldID = worldID
                    });
                    conn.WorldID = worldID;
                }

                WorldData? data = _WorldDataRepository.Get(worldID);
                if (data != null) {
                    // If the data was generated too long ago, don't send the data
                    TimeSpan diff = DateTime.UtcNow - data.Timestamp;
                    if (diff < TimeSpan.FromMinutes(5)) {
                        await Clients.Caller.UpdateData(data);
                    }
                }
            }
        }

    }
}
