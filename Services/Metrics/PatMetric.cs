using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class PatMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Value;

        public PatMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.Pat");

            _Value = _Meter.CreateCounter<long>("honu.pat.value");
        }

        public void RecordValue(long value) {
            _Value.Add(1);
        }

    }
}
