using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class PatHub : Hub<IPatHub> {

        private readonly ILogger<PatHub> _Logger;
        private readonly PatRepository _PatRepository;

        private static Dictionary<string, DateTime> _Velocity = new();

        private static long _ValueCache = 0;

        public PatHub(ILogger<PatHub> logger,
            PatRepository patRepository) {

            _Logger = logger;
            _PatRepository = patRepository;
        }

        public override Task OnDisconnectedAsync(Exception? exception) {
            string connId = Context.ConnectionId;
            _Velocity.Remove(connId);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task<long> Press() {
            string connId = Context.ConnectionId;
            DateTime? lastPat = _Velocity.GetValueOrDefault(connId);

            // max 20 per second per connection
            if (lastPat != null && ((DateTime.UtcNow - lastPat) <= TimeSpan.FromMilliseconds(50))) {
                return _ValueCache;
            }

            long value = await _PatRepository.Incremenent();
            if (value > _ValueCache) {
                _ValueCache = value;
            }
            _Velocity[connId] = DateTime.UtcNow;

            await Clients.All.SendValue(value);

            return value;
        }

        public async Task<long> GetValue() {
            return await _PatRepository.GetValue();
        }

    }
}
