using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class CensusMetric {

        private readonly Meter _Meter;

        private readonly Histogram<double> _Duration;
        private readonly Counter<long> _Calls;

        public CensusMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.Census");
            _Duration = _Meter.CreateHistogram<double>(
                name: "honu.census.duration",
                description: "duration of census calls per collection"
            );
            _Calls = _Meter.CreateCounter<long>(
                name: "honu.census.calls",
                description: "number of calls to a census collection"
            );
        }

        /// <summary>
        ///     record the duration a Census call took
        /// </summary>
        /// <param name="collection">collection of the Census query</param>
        /// <param name="durationSec">how many seconds it took for this Census query to complete</param>
        public void RecordDuration(string collection, double durationSec) {
            _Duration.Record(durationSec,
                new KeyValuePair<string, object?>("collection", collection)
            );
        }

        /// <summary>
        ///     record a call to a Census collection
        /// </summary>
        /// <param name="collection">name of the Census collection a query was made to</param>
        public void RecordCount(string collection) {
            _Calls.Add(1,
                new KeyValuePair<string, object?>("collection", collection)
            );
        }

    }
    
}
