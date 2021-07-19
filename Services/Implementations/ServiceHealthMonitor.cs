using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using watchtower.Models;

namespace watchtower.Services.Implementations {

    public class ServiceHealthMonitor : IServiceHealthMonitor {

        private readonly ILogger<ServiceHealthMonitor> _Logger;

        private ConcurrentDictionary<string, ServiceHealthEntry> _Entries = new ConcurrentDictionary<string, ServiceHealthEntry>();

        public ServiceHealthMonitor(ILogger<ServiceHealthMonitor> logger) {
            _Logger = logger;
        }

        public ServiceHealthEntry? Get(string serviceName) {
            lock (_Entries) {
                _Entries.TryGetValue(serviceName, out ServiceHealthEntry? entry);
                return entry;
            }
        }

        public void Set(string serviceName, ServiceHealthEntry entry) {
            lock (_Entries) {
                _Entries.AddOrUpdate(serviceName, entry, (key, value) => entry);
            }
        }

        public List<string> GetServices() {
            lock (_Entries) {
                return new List<string>(_Entries.Keys);
            }
        }

    }

}