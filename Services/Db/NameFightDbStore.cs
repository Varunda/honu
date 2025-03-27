
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Api;

namespace watchtower.Services.Db {

    public class NameFightDbStore {

        private readonly ILogger<NameFightDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<NameFightEntry> _Reader;

        public NameFightDbStore(ILogger<NameFightDbStore> logger,
            IDbHelper dbHelper, IDataReader<NameFightEntry> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        public async Task<List<NameFightEntry>> Get() {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT 
                    NOW() at time zone 'utc' ""timestamp"",
                    world_id,
                    COUNT(*) ""total"",
                    COUNT(*) filter (WHERE victor_faction_id = 1) ""wins_vs"",
                    COUNT(*) filter (WHERE victor_faction_id = 2) ""wins_nc"",
                    COUNT(*) filter (WHERE victor_faction_id = 3) ""wins_tr""
                FROM alerts 
                WHERE timestamp >= '2025-03-28T07:00' AND timestamp < '2025-03-31T07:00'
                    AND world_id IN (1, 10, 13, 17)
                    AND zone_id <> 0
                    AND victor_faction_id IN (1, 2, 3)
                group by world_id;
            ");

            List<NameFightEntry> entry = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return entry;
        }

    }
}
