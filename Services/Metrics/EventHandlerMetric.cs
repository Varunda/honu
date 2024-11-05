using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using watchtower.Realtime;

namespace watchtower.Services.Metrics {

    public class EventHandlerMetric {

        private readonly Meter _Meter;
        private readonly Counter<long> _EventCount;
        private readonly Histogram<double> _EventDuration;
        private readonly Counter<long> _Duplicate;
        private readonly ObservableGauge<double> _ProcessLag;

        private IEventHandler? _EventHandler;

        public EventHandlerMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.EventHandler");

            _EventCount = _Meter.CreateCounter<long>(
                name: "honu.realtime.events",
                description: "how many events have been processed"
            );

            _EventDuration = _Meter.CreateHistogram<double>(
                name: "honu.realtime.event.duration",
                description: "duration of processing realtime events"
            );

            _Duplicate = _Meter.CreateCounter<long>(
                name: "honu.realtime.duplicate",
                description: "rate of duplicate events"
            );

            _ProcessLag = _Meter.CreateObservableGauge("honu.realtime.process_lag", () => {
                if (_EventHandler == null) {
                    return 0d;
                }
                return (DateTime.UtcNow - _EventHandler.MostRecentProcess()).TotalSeconds;
            });
        }

        public void SetEventHandler(IEventHandler eventHandler) {
            _EventHandler = eventHandler;
        }

        public void RecordEvent(string type, short? worldID) {
            _EventCount.Add(1,
                new KeyValuePair<string, object?>("type", type),
                new KeyValuePair<string, object?>("world", worldID)
            );
        }

        public void RecordEventDuration(string type, short? worldID, double durationSec) {
            _EventDuration.Record(durationSec,
                new KeyValuePair<string, object?>("type", type),
                new KeyValuePair<string, object?>("world", worldID)
            );
        }

        public void RecordDuplicate(string type, short? worldID) {
            _Duplicate.Add(1,
                new KeyValuePair<string, object?>("type", type),
                new KeyValuePair<string, object?>("world", worldID)
            );
        }

    }
}
