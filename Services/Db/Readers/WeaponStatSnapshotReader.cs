using Npgsql;
using System.Data;
using watchtower.Models;

namespace watchtower.Services.Db.Readers {

    public class WeaponStatSnapshotReader : IDataReader<WeaponStatSnapshot> {

        public override WeaponStatSnapshot? ReadEntry(NpgsqlDataReader reader) {
            WeaponStatSnapshot snapshot = new WeaponStatSnapshot();

            snapshot.ID = reader.GetInt64("id");
            snapshot.ItemID = reader.GetInt32("item_id");
            snapshot.Timestamp = reader.GetDateTime("timestamp");
            snapshot.Users = reader.GetInt32("users");
            snapshot.Kills = reader.GetInt64("kills");
            snapshot.Deaths = reader.GetInt64("deaths");
            snapshot.Shots = reader.GetInt64("shots");
            snapshot.ShotsHit = reader.GetInt64("shots_hit");
            snapshot.Headshots = reader.GetInt64("headshots");
            snapshot.VehicleKills = reader.GetInt64("vehicle_kills");
            snapshot.SecondsWith = reader.GetInt64("seconds_with");

            return snapshot;
        }

    }
}
