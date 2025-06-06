using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class PatRepository {

        private readonly ILogger<PatRepository> _Logger;

        private readonly PatDbStore _PatDb;

        private static long _Value = 0;
        private static bool _Loaded = false;
        private static DateTime _LastUpdated = DateTime.MinValue;

        public PatRepository(ILogger<PatRepository> logger,
            PatDbStore patDb) {

            _Logger = logger;
            _PatDb = patDb;
        }

        public async Task<long> Incremenent() {
            Interlocked.Increment(ref _Value);
                
            if ((DateTime.UtcNow - _LastUpdated) >= TimeSpan.FromSeconds(5)) {
                _Logger.LogDebug($"saving pat value to DB [value={_Value}] [last updated={_LastUpdated:u}]");
                await _PatDb.SetValue(_Value);
                _LastUpdated = DateTime.UtcNow;
            }

            return _Value;
        }

        public async Task<long> GetValue() {
            if (_Loaded == false) {
                _Logger.LogDebug($"value is not loaded, getting from DB");
                _Value = await _PatDb.GetValue();
                _Logger.LogInformation($"loaded value from DB [value={_Value}]");
                _Loaded = true;
            }

            return _Value;
        }

    }
}
