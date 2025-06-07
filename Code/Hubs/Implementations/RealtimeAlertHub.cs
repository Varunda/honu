using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.RealtimeAlert;
using watchtower.Services.Metrics;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class RealtimeAlertHub : Hub<IRealtimeAlertHub> {

        private readonly ILogger<RealtimeAlertHub> _Logger;
        private readonly HubMetric _HubMetric;

        private readonly RealtimeAlertRepository _RealtimeAlertRepository;

        private readonly Dictionary<string, string> _GroupMembership = new(); // <connection id, group>
        private readonly Dictionary<string, string> _ControlCodes = new(); // <connection id, code>

        public RealtimeAlertHub(ILogger<RealtimeAlertHub> logger,
            RealtimeAlertRepository realtimeAlertRepository, HubMetric hubMetric) {

            _Logger = logger;
            _HubMetric = hubMetric;
            _RealtimeAlertRepository = realtimeAlertRepository;
        }

        public override Task OnConnectedAsync() {
            _HubMetric.RecordConnect("realtime-alert");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            _HubMetric.RecordDisconnect("realtime-alert");
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        ///     Subscribe to updates to a realtime alert, and unsubscribe from all others
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="zoneID">ID of the zone</param>
        public async Task Subscribe(short worldID, uint zoneID) {
            if (_GroupMembership.TryGetValue(Context.ConnectionId, out string? previousGroup) == true) {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousGroup!);
                _Logger.LogTrace($"Removed {Context.ConnectionId} from {previousGroup}");
            }

            RealtimeAlert? alert = _RealtimeAlertRepository.Get(worldID, zoneID);
            if (alert == null) {
                return;
            }

            string groupID = $"RealtimeAlert.{worldID}.{zoneID}";
            _GroupMembership[Context.ConnectionId] = groupID;
            await Groups.AddToGroupAsync(Context.ConnectionId, groupID);
        }

        /// <summary>
        ///     Set the control code this connection will listen to
        /// </summary>
        /// <param name="code">Control code to use</param>
        public async Task SetControlCode(string code) {
            string connID = Context.ConnectionId;

            if (_ControlCodes.TryGetValue(connID, out string? previousCode) == true) {
                await Groups.RemoveFromGroupAsync(connID, previousCode!);
                _ControlCodes.Remove(connID);
            }

            string group = $"RealtimeAlert.ControlCode.{code}";

            _ControlCodes[connID] = group;
            await Groups.AddToGroupAsync(connID, group);
        }

        /// <summary>
        ///     Tell all clients that have that control code set to perform this action
        /// </summary>
        /// <param name="code">Control code to tell</param>
        /// <param name="action">Action to be performed</param>
        public async Task RemoteControlCall(string code, string action) {
            string group = $"RealtimeAlert.ControlCode.{code}";
            await Clients.Group(group).RemoteCall(action);
        }

    }
}
