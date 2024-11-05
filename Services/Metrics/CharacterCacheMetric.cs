using System.Diagnostics.Metrics;

namespace watchtower.Services.Metrics {

    public class CharacterCacheMetric {

        private readonly Meter _Meter;

        private readonly Counter<long> _InMemory;
        private readonly Counter<long> _InDb;
        private readonly Counter<long> _InCensus;

        public CharacterCacheMetric(IMeterFactory factory) {
            _Meter = factory.Create("Honu.CharacterCache");

            _InMemory = _Meter.CreateCounter<long>(
                name: "honu.charactercache.memory",
                description: "how many character looks were found in memory"
            );

            _InDb = _Meter.CreateCounter<long>(
                name: "honu.charactercache.db",
                description: "how many character looks were found from DB"
            );

            _InCensus = _Meter.CreateCounter<long>(
                name: "honu.charactercache.census",
                description: "how many character looks were found from Census"
            );
        }

        public void RecordMemory() {
            _InMemory.Add(1);
        }

        public void RecordDb() {
            _InDb.Add(1);
        }

        public void RecordCensus() {
            _InCensus.Add(1);
        }

    }
}
