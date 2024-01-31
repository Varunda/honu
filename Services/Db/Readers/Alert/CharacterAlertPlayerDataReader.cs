using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class CharacterAlertPlayerDataReader : IDataReader<CharacterAlertPlayer> {

        public override CharacterAlertPlayer? ReadEntry(NpgsqlDataReader reader) {
            CharacterAlertPlayer cap = new();

            cap.Duration = reader.GetInt32("duration");
            cap.ZoneID = reader.GetUInt32("zone_id");
            cap.WorldID = reader.GetInt16("world_id");
            cap.VictorFactionID = reader.GetNullableInt16("victor_faction_id");
            cap.MetagameAlertID = reader.GetInt32("metagame_alert_id");
            cap.InstanceID = reader.GetInt32("instance_id");
            cap.Name = reader.GetString("name");

            cap.AlertID = reader.GetInt64("alert_id");
            cap.Timestamp = reader.GetDateTime("timestamp");

            cap.CharacterID = reader.GetString("character_id");
            cap.OutfitID = reader.GetNullableString("outfit_id");
            cap.SecondsOnline = reader.GetInt32("seconds_online");

            cap.Kills = reader.GetInt32("kills");
            cap.Deaths = reader.GetInt32("deaths");
            cap.VehicleKills = reader.GetInt32("vehicle_kills");

            cap.Heals = reader.GetInt32("heals");
            cap.Revives = reader.GetInt32("revives");
            cap.ShieldRepairs = reader.GetInt32("shield_repairs");
            cap.Resupplies = reader.GetInt32("resupplies");
            cap.Repairs = reader.GetInt32("repairs");
            cap.Spawns = reader.GetInt32("spawns");

            return cap;
        }

    }
}
