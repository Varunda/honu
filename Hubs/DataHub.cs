using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Hubs {

    public class DataHub : Hub {

        public DataHub() {

        }

        public Task UpdateWorld(WorldData data) {
            return Clients.All.SendAsync("UpdateWorld", data);
        }

    }
}
