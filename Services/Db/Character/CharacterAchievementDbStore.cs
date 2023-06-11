using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterAchievementDbStore {

        private readonly ILogger<CharacterAchievementDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterAchievement> _Reader;

        public CharacterAchievementDbStore(ILogger<CharacterAchievementDbStore> logger,
            IDbHelper helper, IDataReader<CharacterAchievement> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<CharacterAchievement>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_achievement
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterAchievement> achs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return achs;
        }

        public async Task Upsert(CharacterAchievement ach) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_achievement (
                    character_id, achievement_id, earned_count, start_date, finish_date, last_save_date
                ) VALUES (
                    @CharacterID, @AchievementID, @EarnedCount, @StartDate, @FinishDate, @LastSaveDate
                ) ON CONFLICT (character_id, achievement_id) DO
                    UPDATE SET earned_count = @EarnedCount,
                        start_date = @StartDate,
                        finish_date = @FinishDate,
                        last_save_date = @LastSaveDate;
            ");

            cmd.AddParameter("CharacterID", ach.CharacterID);
            cmd.AddParameter("AchievementID", ach.AchievementID);
            cmd.AddParameter("EarnedCount", ach.EarnedCount);
            cmd.AddParameter("StartDate", ach.StartDate);
            cmd.AddParameter("FinishDate", ach.FinishDate);
            cmd.AddParameter("LastSaveDate", ach.LastSaveDate);
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
