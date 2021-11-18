using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Db.Implementations {

    public class BattleRankDbStore : IBattleRankDbStore {

        private readonly ILogger<BattleRankDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public BattleRankDbStore(ILogger<BattleRankDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task Insert(string charID, int rank, DateTime timestamp) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO battle_rank (
                    character_id, rank, timestamp
                ) VALUES (
                    @CharacterID, @BattleRank, @Timestamp
                );
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("BattleRank", rank);
            cmd.AddParameter("Timestamp", timestamp);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
