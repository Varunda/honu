using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Realtime;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Commands {

    [Command]
    public class RealtimeCommand {

        private readonly ILogger<RealtimeCommand> _Logger;
        private readonly IRealtimeMonitor _RealtimeMonitor;

        public RealtimeCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<RealtimeCommand>>();
            _RealtimeMonitor = services.GetRequiredService<IRealtimeMonitor>();
        }

        public void Restart() {
            _Logger.LogInformation($"Resubscribing to realtime");
            _RealtimeMonitor.Resubscribe();
            _Logger.LogInformation($"Resubscribed to realtime");
        }

    }

}