using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services {

    /// <summary>
    /// Service that holds the health of long running hosted services
    /// </summary>
    public interface IServiceHealthMonitor {

        ServiceHealthEntry? Get(string serviceName);

        void Set(string serviceName, ServiceHealthEntry entry);

        List<string> GetServices();

    }

    public static class IServiceHealthMonitorExtensions {

        /// <summary>
        ///     Get the <see cref="ServiceHealthEntry"/> with <see cref="ServiceHealthEntry.Name"/> of <paramref name="serviceName"/>,
        ///     creating it if it does not exist
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static ServiceHealthEntry GetOrCreate(this IServiceHealthMonitor monitor, string serviceName) {
            return monitor.Get(serviceName) ?? new ServiceHealthEntry() {
                Name = serviceName
            };
        }

    }

}