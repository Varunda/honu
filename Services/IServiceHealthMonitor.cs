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

}