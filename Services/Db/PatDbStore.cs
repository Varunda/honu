using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Db {

    public class PatDbStore {

        private readonly ILogger<PatDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public PatDbStore(ILogger<PatDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<long> GetValue() {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT value FROM pat_silzz;
            ");

            await cmd.PrepareAsync();
            long value = await cmd.ExecuteInt64(CancellationToken.None);

            return value;
        }

        public async Task SetValue(long value) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE pat_silzz
                    SET value = @Value,
                        timestamp = NOW() at time zone 'utc'
                    WHERE value < @Value;
            ");

            cmd.AddParameter("Value", value);
            await cmd.PrepareAsync();

            _Logger.LogDebug($"setting pat value [value={value}]");

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
