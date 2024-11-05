using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class RealtimeStreamMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Reconnect;

        public RealtimeStreamMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.RealtimeStream");

            _Reconnect = _Meter.CreateCounter<long>(
                name: "honu.realtimestream.reconnect",
                description: "reconnections on realtime streams per world"
            );
        }

        public void RecordReconnect(short worldID) {
            _Reconnect.Add(1, new KeyValuePair<string, object?>("worldID", worldID));
        }

    }
}
