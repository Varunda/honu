using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class WeaponStatTopReader : IDataReader<WeaponStatTop> {

        public override WeaponStatTop? ReadEntry(NpgsqlDataReader reader) {
            WeaponStatTop top = new();

            top.ID = reader.GetInt64("id");
            top.ItemID = reader.GetInt32("item_id");
            top.TypeID = reader.GetInt16("type_id");
            top.WorldID = reader.GetInt16("world_id");
            top.FactionID = reader.GetInt16("faction_id");
            top.ReferenceID = reader.GetInt64("reference_id");
            top.Timestamp = reader.GetDateTime("timestamp");

            return top;
        }

    }
}
