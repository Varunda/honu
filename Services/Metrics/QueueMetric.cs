using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class QueueMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Count;
        private readonly Histogram<double> _Duration;

        public QueueMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.Queue");

            _Count = _Meter.CreateCounter<long>(
                name: "honu_queue_count",
                description: "how many items have been inserted into the queue"
            );

            _Duration = _Meter.CreateHistogram<double>(
                name: "honu_queue_duration",
                description: "historgram of how long it takes to process an entry within the queue",
                unit: "s"
                /*
                advice: new InstrumentAdvice<double> { 
                    HistogramBucketBoundaries = [ 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 ]
                }*/
            );
        }

        public void RecordCount(string queue) {
            _Count.Add(1,
                new KeyValuePair<string, object?>("queue", queue)
            );
        }

        public void RecordDuration(string queue, double durationSec) {
            // we can't change the bucket boundaries, so we make useful data
            _Duration.Record(durationSec, 
                new KeyValuePair<string, object?>("queue", queue)
            );
        }

    }
}
