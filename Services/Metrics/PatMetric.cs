using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class PatMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Value;
        private readonly Counter<long> _VelocityRejected;
        private readonly Counter<long> _GlobalUpdateRejected;

        public PatMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.Pat");

            _Value = _Meter.CreateCounter<long>(
                name: "honu.pat.value",
                description: "how many times a press increment has occurred, not the actual pat value"
            );
            _VelocityRejected = _Meter.CreateCounter<long>(
                name: "honu.pat.velocity-rejected",
                description: "how many pats were rejected due to too many per second"
            );
            _GlobalUpdateRejected = _Meter.CreateCounter<long>(
                name:"honu.pat.global-update-rejected",
                description: "how many global updates were not sent due to too many per second"
            );
        }

        public void RecordValue(long value) {
            _Value.Add(1);
        }

        public void RecordVelocityReject() {
            _VelocityRejected.Add(1);
        }

        public void RecordGlobalUpdateReject() {
            _GlobalUpdateRejected.Add(1);
        }

    }
}
