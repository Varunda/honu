using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class ItemAddedEventReader : IDataReader<ItemAddedEvent> {

        public override ItemAddedEvent? ReadEntry(NpgsqlDataReader reader) {
            ItemAddedEvent ev = new();

            ev.ID = reader.GetInt64("id");
            ev.CharacterID = reader.GetString("character_id");
            ev.ItemID = reader.GetInt32("item_id");
            ev.Context = reader.GetString("context");
            ev.ItemCount = reader.GetInt32("item_count");
            ev.Timestamp = reader.GetDateTime("timestamp");
            ev.ZoneID = reader.GetUInt32("zone_id");
            ev.WorldID = reader.GetInt16("world_id");

            return ev;
        }
    }
}
