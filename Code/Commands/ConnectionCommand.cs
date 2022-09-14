using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models;

namespace watchtower.Code.Commands {

    [Command]
    public class ConnectionCommand {

        private readonly ILogger<ConnectionCommand> _Logger;

        public ConnectionCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ConnectionCommand>>();
        }

        public void List() {
            lock (ConnectionStore.Get().Connections) {
                string s = $"Connections: {ConnectionStore.Get().Connections.Count}\n";

                foreach (KeyValuePair<string, TrackedConnection> entry in ConnectionStore.Get().Connections) {
                    s += $"\t{entry.Key}: {entry.Value.WorldID}#{entry.Value.Duration}\t{entry.Value.ConnectedAt}\n";
                }

                _Logger.LogInformation(s);
            }
        }

        public void World() {
            lock (ConnectionStore.Get().Connections) {
                string s = $"Connections: {ConnectionStore.Get().Connections.Count}\n";

                int conneryCount = 0;
                int cobaltCount = 0;
                int emeraldCount = 0;
                int jaegerCount = 0;
                int millerCount = 0;
                int soltechCount = 0;
                int otherCount = 0;

                foreach (KeyValuePair<string, TrackedConnection> entry in ConnectionStore.Get().Connections) {
                    if (entry.Value.WorldID == 1) {
                        ++conneryCount;
                    } else if (entry.Value.WorldID == 10) {
                        ++millerCount;
                    } else if (entry.Value.WorldID == 13) {
                        ++cobaltCount;
                    } else if (entry.Value.WorldID == 17) {
                        ++emeraldCount;
                    } else if (entry.Value.WorldID == 19) {
                        ++jaegerCount;
                    } else if (entry.Value.WorldID == 40) {
                        ++soltechCount;
                    } else {
                        ++otherCount;
                    }
                }

                s += $"\tConnery: {conneryCount}\n\tCobalt: {cobaltCount}\n\tEmerald: {emeraldCount}\n\tJaeger: {jaegerCount}\n\tMiller: {millerCount}\n\tSolTech: {soltechCount}\n\tOther: {otherCount}\n";

                _Logger.LogInformation(s);
            }
        }

    }
}
