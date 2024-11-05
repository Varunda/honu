using System;
using System.Diagnostics.Metrics;
using watchtower.Realtime;

namespace watchtower.Services.Metrics {

    public class EventQueueMetric {

        private readonly IEventHandler _EventHandler;

        private readonly Meter _Meter;
        private readonly ObservableUpDownCounter<double> _Event;

        public EventQueueMetric(IMeterFactory factory,
            IEventHandler eventHandler) {

            _EventHandler = eventHandler;

            _Meter = factory.Create("Honu.EventQueue");
            _Event = _Meter.CreateObservableUpDownCounter<double>("honu.process_lag.duration", () => {
                return (DateTime.UtcNow - _EventHandler.MostRecentProcess()).TotalSeconds;
            });
        }

    }
}
