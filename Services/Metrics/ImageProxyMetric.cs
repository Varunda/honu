using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class ImageProxyMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _Proxied;
        private readonly Counter<long> _Fetched;

        public ImageProxyMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.ImageProxy");

            _Proxied = _Meter.CreateCounter<long>(
                name: "honu.imageproxy.hit",
                description: "how many requests for a Census image hit the Honu cache"
            );

            _Fetched = _Meter.CreateCounter<long>(
                name: "honu.imageproxy.miss",
                description: "how many requests for a Census image had to be fetched from Census"
            );
        }

        public void RecordHit() {
            _Proxied.Add(1);
        }

        public void RecordMiss() {
            _Fetched.Add(1);
        }

    }
}
