using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Commands {

    [Command]
    public class ServiceCommand {

        private readonly ILogger<ServiceCommand> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        public ServiceCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ServiceCommand>>();
            _ServiceHealthMonitor = services.GetRequiredService<IServiceHealthMonitor>();
        }

        public void Print() {
            List<string> services = _ServiceHealthMonitor.GetServices();

            string s = $"Services running: {services.Count}\n";
            s += $"{"service",-24} | {"last ran",30} | {"time ago",10} | {"run time",12} | {"message",160}\n";

            DateTimeOffset now = DateTimeOffset.UtcNow;

            foreach (string service in services) {
                ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(service);

                if (entry != null) {
                    TimeSpan timeAgo = now - entry.LastRan;
                    string ago = $"{timeAgo.Minutes:D2}:{timeAgo.Seconds:D2}";

                    s += $"{entry.Name,-24} | {entry.LastRan,30} | {ago,10} | {entry.RunDuration,10}ms | {entry.Message?.Truncate(160)}\n";
                }
            }

            _Logger.LogInformation(s);
        }

        public void Enable(string serviceName) {
            ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(serviceName);
            if (entry == null) {
                _Logger.LogWarning($"Cannot enable service '{serviceName}': does not exist");
            } else {
                entry.Enabled = true;
                _ServiceHealthMonitor.Set(serviceName, entry);
                _Logger.LogInformation($"Enabled service '{serviceName}'");
            }
        }

        public void Disable(string serviceName) {
            ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(serviceName);

            if (entry == null) {
                entry = new ServiceHealthEntry() {
                    Name = serviceName
                };
            }

            entry.Enabled = false;
            _ServiceHealthMonitor.Set(serviceName, entry);
            _Logger.LogInformation($"Disabled service '{serviceName}'");
        }

    }

}