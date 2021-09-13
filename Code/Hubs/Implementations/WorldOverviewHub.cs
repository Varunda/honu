using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.Hubs.Implementations {

    public class WorldOverviewHub : Hub<IWorldOverviewHub> {

        private readonly ILogger<WorldOverviewHub> _Logger;

        public WorldOverviewHub(ILogger<WorldOverviewHub> logger) {
            _Logger = logger;
        }

    }
}
