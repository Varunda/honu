using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterStatDbStore : IDataReader<PsCharacterStat> {

        private readonly ILogger<CharacterStatDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterStatDbStore(ILogger<CharacterStatDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Get the character stats for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns></returns>
        public async Task<List<PsCharacterStat>> GetByID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_stats
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<PsCharacterStat> stats = await ReadList(cmd);
            await conn.CloseAsync();

            return stats;
        }

        /// <summary>
        ///     Set the character stats of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="stats">List of stats the character has</param>
        public async Task Set(string charID, List<PsCharacterStat> stats) {
            if (stats.Count == 0) {
                return;
            }

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                BEGIN;

                INSERT INTO character_stats(character_id, stat_name, profile_id, value_forever, value_monthly, value_weekly, value_daily, value_max_one_life, timestamp)
                    VALUES {string.Join(",\n", stats.Select(iter => $"('{iter.CharacterID}', '{iter.StatName}', {iter.ProfileID}, "
                    + $"{iter.ValueForever}, {iter.ValueMonthly}, {iter.ValueWeekly}, {iter.ValueDaily}, {iter.ValueMaxOneLife}, '{iter.Timestamp}')"))};

                COMMIT;
            ");

            cmd.AddParameter("CharacterID", charID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override PsCharacterStat? ReadEntry(NpgsqlDataReader reader) {
            PsCharacterStat stat = new PsCharacterStat();

            stat.CharacterID = reader.GetString("character_id");
            stat.StatName = reader.GetString("stat_name");
            stat.ProfileID = reader.GetInt32("profile_id");
            stat.ValueForever = reader.GetInt32("value_forever");
            stat.ValueMonthly = reader.GetInt32("value_monthly");
            stat.ValueWeekly = reader.GetInt32("value_weekly");
            stat.ValueDaily = reader.GetInt32("value_daily");
            stat.ValueMaxOneLife = reader.GetInt32("value_max_one_life");
            stat.Timestamp = reader.GetDateTime("timestamp");

            return stat;
        }
    }
}
