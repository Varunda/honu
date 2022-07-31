using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class AchievementEarnedDbStore {

        private readonly ILogger<AchievementEarnedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public AchievementEarnedDbStore(ILogger<AchievementEarnedDbStore> logger, IDbHelper dbHelper) {
            _Logger = logger;
            _DbHelper = dbHelper;
        }

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

    }
}
