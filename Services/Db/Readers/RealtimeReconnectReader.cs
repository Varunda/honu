using Npgsql;
using System.Data;
using watchtower.Models.Health;

namespace watchtower.Services.Db.Readers {

    public class RealtimeReconnectReader : IDataReader<RealtimeReconnectEntry> {

        public override RealtimeReconnectEntry? ReadEntry(NpgsqlDataReader reader) {
            RealtimeReconnectEntry entry = new RealtimeReconnectEntry();

            entry.ID = reader.GetInt64("id");
            entry.WorldID = reader.GetInt16("world_id");
            entry.Timestamp = reader.GetDateTime("timestamp");
            entry.StreamType = reader.GetString("stream_type");
            entry.FailedCount = reader.GetInt32("failed_count");
            entry.Duration = reader.GetInt32("duration");
            entry.EventCount = reader.GetInt32("event_count");

            return entry;
        }

    }
}
