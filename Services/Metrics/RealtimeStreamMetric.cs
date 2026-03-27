using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class RealtimeStreamMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Reconnect;
        private readonly Counter<long> _ReconnectException;

        public RealtimeStreamMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.RealtimeStream");

            _Reconnect = _Meter.CreateCounter<long>(
                name: "honu.realtimestream.reconnect",
                description: "reconnections on realtime streams per world"
            );

            _ReconnectException = _Meter.CreateCounter<long>(
                name: "honu.realtimestream.reconnect-exception",
                description: "how many reconnections fail on a realtime stream per world"
            );
        }

        public void RecordReconnect(short worldID) {
            _Reconnect.Add(1, new KeyValuePair<string, object?>("worldID", worldID));
        }

        public void RecordReconnectException(short worldID) {
            _ReconnectException.Add(1, new KeyValuePair<string, object?>("worldID", worldID));
        }

    }
}
