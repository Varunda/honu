using Npgsql;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class WeaponStatBucketReader : IDataReader<WeaponStatBucket> {

        public override WeaponStatBucket? ReadEntry(NpgsqlDataReader reader) {
            WeaponStatBucket bucket = new();

            bucket.ID = reader.GetInt64("id");
            bucket.ItemID = reader.GetInt32("item_id");
            bucket.TypeID = reader.GetInt16("type_id");
            bucket.Timestamp = reader.GetDateTime("timestamp");
            bucket.Start = reader.GetDouble("start");
            bucket.Width = reader.GetDouble("width");
            bucket.Count = reader.GetInt32("count");

            return bucket;
        }

    }
}
