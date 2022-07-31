using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class AchievementEarnedDbStore {

        private readonly ILogger<AchievementEarnedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<AchievementEarnedEvent> _Reader;

        public AchievementEarnedDbStore(ILogger<AchievementEarnedDbStore> logger,
            IDbHelper dbHelper, IDataReader<AchievementEarnedEvent> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Insert a new <see cref="AchievementEarnedEvent"/> to the DB, getting the ID of the row that was inserted
        /// </summary>
        /// <param name="ev">Parameters used to insert</param>
        /// <returns>
        ///     The <see cref="AchievementEarnedEvent.ID"/> of the row that was just inserted
        /// </returns>
        public async Task<long> Insert(AchievementEarnedEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO achievement_earned (
                    character_id, achievement_id, timestamp, zone_id, world_id
                ) VALUES (
                    @CharacterID, @AchievementID, @Timestamp, @ZoneID, @WorldID
                ) RETURNING id;
            ");

            cmd.AddParameter("CharacterID", ev.CharacterID);
            cmd.AddParameter("AchievementID", ev.AchievementID);
            cmd.AddParameter("Timestamp", ev.Timestamp);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("WorldID", ev.WorldID);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        /// <summary>
        ///     Get the <see cref="AchievementEarnedEvent"/> that a player earned during a period of time
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="start">When to start searching</param>
        /// <param name="end">When to end searching</param>
        /// <returns>
        ///     A list of <see cref="AchievementEarnedEvent"/>s that occured between <paramref name="start"/>
        ///     and <paramref name="end"/> and were earned by the character with ID of <paramref name="charID"/>
        /// </returns>
        public async Task<List<AchievementEarnedEvent>> GetByCharacterIDAndRange(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM achievement_earned
                    WHERE character_id = @CharacterID
                        AND timestamp BETWEEN @PeriodStart AND @PeriodEnd;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<AchievementEarnedEvent> events = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return events;
        }

    }
}
