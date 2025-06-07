using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Services.Metrics;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class PatHub : Hub<IPatHub> {

        private readonly ILogger<PatHub> _Logger;
        private readonly PatRepository _PatRepository;
        private readonly HubMetric _HubMetric;
        private readonly PatMetric _PatMetric;

        private static ConcurrentDictionary<string, DateTime> _Velocity = new();

        private static long _ValueCache = 0;

        private static DateTime _LastGlobalUpdate = DateTime.MinValue;

        public PatHub(ILogger<PatHub> logger,
            PatRepository patRepository, HubMetric hubMetric,
            PatMetric patMetric) {

            _Logger = logger;
            _PatRepository = patRepository;
            _HubMetric = hubMetric;
            _PatMetric = patMetric;
        }

        public override Task OnConnectedAsync() {
            _HubMetric.RecordConnect("pat");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            string connId = Context.ConnectionId;
            _Velocity.Remove(connId, out _);

            _HubMetric.RecordDisconnect("pat");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task<long> Press() {
            string connId = Context.ConnectionId;
            DateTime? lastPat = _Velocity.GetValueOrDefault(connId);

            // max 20 per second per connection
            if (lastPat != null && ((DateTime.UtcNow - lastPat) <= TimeSpan.FromMilliseconds(50))) {
                _PatMetric.RecordVelocityReject();
                return _ValueCache;
            }

            long value = await _PatRepository.Incremenent();
            if (value > _ValueCache) {
                _ValueCache = value;
            }
            _PatMetric.RecordValue(value);
            _Velocity[connId] = DateTime.UtcNow;

            // send at most 10 updates per second to other clients
            if ((DateTime.UtcNow - _LastGlobalUpdate) >= TimeSpan.FromMilliseconds(100)) {
                await Clients.Others.SendValue(value);
                _LastGlobalUpdate = DateTime.UtcNow;
            } else {
                _PatMetric.RecordGlobalUpdateReject();
            }

            return value;
        }

        public async Task<long> GetValue() {
            return await _PatRepository.GetValue();
        }

    }
}
