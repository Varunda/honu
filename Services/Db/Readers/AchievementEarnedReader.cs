using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class AchievementEarnedReader : IDataReader<AchievementEarnedEvent> {

        public override AchievementEarnedEvent? ReadEntry(NpgsqlDataReader reader) {
            AchievementEarnedEvent ev = new AchievementEarnedEvent();

            ev.ID = reader.GetInt64("id");
            ev.CharacterID = reader.GetString("character_id");
            ev.AchievementID = reader.GetInt32("achievement_id");
            ev.Timestamp = reader.GetDateTime("timestamp");
            ev.ZoneID = reader.GetUInt32("zone_id");
            ev.WorldID = reader.GetInt16("world_id");

            return ev;
        }

    }
}
