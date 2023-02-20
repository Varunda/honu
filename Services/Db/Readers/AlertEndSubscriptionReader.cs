using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    /// <summary>
    ///     database reader to read rows into a <see cref="AlertEndSubscription"/>
    /// </summary>
    public class AlertEndSubscriptionReader : IDataReader<AlertEndSubscription> {

        public override AlertEndSubscription? ReadEntry(NpgsqlDataReader reader) {
            AlertEndSubscription sub = new();

            sub.ID = reader.GetInt64("id");
            sub.CreatedByID = reader.GetUInt64("created_by_id");
            sub.Timestamp = reader.GetDateTime("timestamp");

            sub.GuildID = reader.GetNullableUInt64("guild_id");
            sub.ChannelID = reader.GetNullableUInt64("channel_id");

            sub.WorldID = reader.GetNullableInt16("world_id");
            sub.CharacterID = reader.GetNullableString("character_id");
            sub.OutfitID = reader.GetNullableString("outfit_id");

            sub.WorldCharacterMinimum = reader.GetInt32("world_character_minimum");
            sub.OutfitCharacterMinimum = reader.GetInt32("outfit_character_minimum");
            sub.CharacterMinimumSeconds = reader.GetInt32("character_minimum_seconds");

            return sub;
        }

    }
}
