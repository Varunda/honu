using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertPlayerDataReader : IDataReader<AlertPlayerDataEntry> {

        public override AlertPlayerDataEntry? ReadEntry(NpgsqlDataReader reader) {
            AlertPlayerDataEntry entry = new AlertPlayerDataEntry();

            entry.ID = reader.GetInt64("id");
            entry.AlertID = reader.GetInt64("alert_id");

            entry.CharacterID = reader.GetString("character_id");
            entry.OutfitID = reader.GetNullableString("outfit_id");
            entry.SecondsOnline = reader.GetInt32("seconds_online");

            entry.Kills = reader.GetInt32("kills");
            entry.Deaths = reader.GetInt32("deaths");
            entry.VehicleKills = reader.GetInt32("vehicle_kills");

            entry.Heals = reader.GetInt32("heals");
            entry.Revives = reader.GetInt32("revives");
            entry.ShieldRepairs = reader.GetInt32("shield_repairs");
            entry.Resupplies = reader.GetInt32("resupplies");
            entry.Repairs = reader.GetInt32("repairs");
            entry.Spawns = reader.GetInt32("spawns");

            return entry;
        }

    }
}
