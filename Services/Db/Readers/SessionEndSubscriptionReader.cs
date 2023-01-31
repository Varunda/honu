using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class SessionEndSubscriptionReader : IDataReader<SessionEndSubscription> {

        public override SessionEndSubscription? ReadEntry(NpgsqlDataReader reader) {
            SessionEndSubscription sub = new();

            sub.ID = reader.GetInt64("id");
            sub.DiscordID = reader.GetUInt64("discord_id");
            sub.CharacterID = reader.GetString("character_id");
            sub.Timestamp = reader.GetDateTime("timestamp");

            return sub;
        }

    }
}
