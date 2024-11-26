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
        private readonly RealtimeMonitor _RealtimeMonitor;

        public RealtimeCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<RealtimeCommand>>();
            _RealtimeMonitor = services.GetRequiredService<RealtimeMonitor>();
        }

        public void ResubscribeAll() {
            _Logger.LogInformation($"Resubscribing to realtime streams");
            _RealtimeMonitor.ResubscribeAll();
            _Logger.LogInformation($"Resubscribed to realtime streams");
        }

        public async Task ReconnectAll() {
            _Logger.LogInformation($"Reconnecting all realtime streams");
            await _RealtimeMonitor.ReconnectAll();
            _Logger.LogInformation($"Reconnected to all realtime streams");
        }

        public async Task Reconnect(string name) {
            _Logger.LogInformation($"Reconnecting stream: '{name}'");
            await _RealtimeMonitor.Reconnect(name);
        }

        public void List() {
            List<string> streams = _RealtimeMonitor.GetStreamNames();
            _Logger.LogInformation($"Realtime streams ({streams.Count}):");
            foreach (string stream in streams) {
                _Logger.LogInformation($"{stream}");
            }
        }

        public async Task ToggleNss() {
            RealtimeMonitor.UseNss = !RealtimeMonitor.UseNss;
            _Logger.LogInformation($"NSS usage toggle [value={RealtimeMonitor.UseNss}]");
            await _RealtimeMonitor.CreateAllStreams();
        }

        public void GetNss() {
            _Logger.LogInformation($"nss flag [value={RealtimeMonitor.UseNss}]");
        }

    }

}