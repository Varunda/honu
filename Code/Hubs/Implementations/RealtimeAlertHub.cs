using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.RealtimeAlert;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class RealtimeAlertHub : Hub<IRealtimeAlertHub> {

        private readonly ILogger<RealtimeAlertHub> _Logger;
        private readonly RealtimeAlertRepository _RealtimeAlertRepository;

        private readonly Dictionary<string, string> _GroupMembership = new(); // <connection id, group>

        public RealtimeAlertHub(ILogger<RealtimeAlertHub> logger,
            RealtimeAlertRepository realtimeAlertRepository) {

            _Logger = logger;
            _RealtimeAlertRepository = realtimeAlertRepository;
        }

        public async Task GetList() {
            List<RealtimeAlert> alerts = _RealtimeAlertRepository.GetAll();

            List<RealtimeAlert> mini = new(alerts.Count);
            foreach (RealtimeAlert a in alerts) {
                RealtimeAlert alert = new(a.VS, a.NC, a.TR);
                alert.WorldID = a.WorldID;
                alert.ZoneID = a.ZoneID;
                alert.Timestamp = a.Timestamp;
                mini.Add(alert);
            }

            await Clients.Caller.SendAll(mini);
        }

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

            await Clients.Caller.SendFull(alert);
        }

        public async Task GetFull(short worldID, uint zoneID) {
            RealtimeAlert? alert = _RealtimeAlertRepository.Get(worldID, zoneID);
            if (alert == null) {
                return;
            }

            await Clients.Caller.SendFull(alert);
        }

    }
}
