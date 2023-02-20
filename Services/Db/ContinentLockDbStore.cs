using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    /// <summary>
    ///     DB service around the continent_lock table
    /// </summary>
    public class ContinentLockDbStore {

        private readonly ILogger<ContinentLockDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<ContinentLockEntry> _Reader;

        public ContinentLockDbStore(ILogger<ContinentLockDbStore> logger,
            IDbHelper dbHelper, IDataReader<ContinentLockEntry> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get all <see cref="ContinentLockEntry"/> stored in the DB.
        ///     If an enty for a world//zone doesn't exist, it has not been set
        /// </summary>
        public async Task<List<ContinentLockEntry>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM continent_lock;
            ");

            List<ContinentLockEntry> entries = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Update/Insert a <see cref="ContinentLockEntry"/> in the DB
        /// </summary>
        /// <param name="entry">Parameters to update</param>
        public async Task Upsert(ContinentLockEntry entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO continent_lock (
                    zone_id, world_id, timestamp
                ) VALUES (
                    @ZoneID, @WorldID, @Timestamp
                ) ON CONFLICT (zone_id, world_id) DO
                    UPDATE SET timestamp = @Timestamp
            ");

            cmd.AddParameter("ZoneID", entry.ZoneID);
            cmd.AddParameter("WorldID", entry.WorldID);
            cmd.AddParameter("Timestamp", entry.Timestamp);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
