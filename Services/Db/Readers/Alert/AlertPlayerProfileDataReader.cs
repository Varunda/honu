using Npgsql;
using System.Data;
using watchtower.Models.Alert;

namespace watchtower.Services.Db.Readers.Alert {

    public class AlertPlayerProfileDataReader : IDataReader<AlertPlayerProfileData> {

        public override AlertPlayerProfileData? ReadEntry(NpgsqlDataReader reader) {
            AlertPlayerProfileData data = new AlertPlayerProfileData();

            data.ID = reader.GetInt64("id");
            data.AlertID = reader.GetInt64("alert_id");
            data.CharacterID = reader.GetString("character_id");
            data.ProfileID = reader.GetInt16("profile_id");
            data.Kills = reader.GetInt32("kills");
            data.Deaths = reader.GetInt32("deaths");
            data.VehicleKills = reader.GetInt32("vehicle_kills");
            data.TimeAs = reader.GetInt32("time_as");

            return data;
        }

    }
}
