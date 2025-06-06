using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class HubMetric {

        private readonly Meter _Meter;

        private readonly UpDownCounter<long> _Connections;
        private readonly Counter<long> _ConnectionCount;

        public HubMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.Hub");
            _Connections = _Meter.CreateUpDownCounter<long>(
                name: "honu.hub.connections",
                description: "how many connections are open to a signalR hub"
            );
            _ConnectionCount = _Meter.CreateCounter<long>(
                name: "honu.hub.connection_count",
                description: "total number of connections opened to this hub"
            );
        }

        public void RecordConnect(string hubName) {
            _Connections.Add(1, new KeyValuePair<string, object?>("hub", hubName));
            _ConnectionCount.Add(1, new KeyValuePair<string, object?>("hub", hubName));
        }

        public void RecordDisconnect(string hubName) {
            _Connections.Add(-1, new KeyValuePair<string, object?>("hub", hubName));
        }

    }
}
