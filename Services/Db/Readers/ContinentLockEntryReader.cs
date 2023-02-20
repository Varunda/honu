using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class ContinentLockEntryReader : IDataReader<ContinentLockEntry> {

        public override ContinentLockEntry? ReadEntry(NpgsqlDataReader reader) {
            ContinentLockEntry entry = new();

            entry.ZoneID = reader.GetUInt32("zone_id");
            entry.WorldID = reader.GetInt16("world_id");
            entry.Timestamp = reader.GetDateTime("timestamp");

            return entry;
        }

    }
}
