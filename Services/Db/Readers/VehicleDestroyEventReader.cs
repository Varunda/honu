using Npgsql;
using System.Data;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class VehicleDestroyEventReader : IDataReader<VehicleDestroyEvent> {

        public override VehicleDestroyEvent? ReadEntry(NpgsqlDataReader reader) {
            VehicleDestroyEvent ev = new VehicleDestroyEvent();

            ev.ID = reader.GetInt64("id");
            ev.AttackerCharacterID = reader.GetString("attacker_character_id");
            ev.AttackerFactionID = reader.GetInt16("attacker_faction_id");
            ev.AttackerLoadoutID = reader.GetInt16("attacker_loadout_id");
            ev.AttackerTeamID = reader.GetInt16("attacker_team_id");
            ev.AttackerVehicleID = reader.GetString("attacker_vehicle_id");
            ev.AttackerWeaponID = reader.GetInt32("attacker_weapon_id");
            ev.KilledCharacterID = reader.GetString("killed_character_id");
            ev.KilledFactionID = reader.GetInt16("killed_faction_id");
            ev.KilledTeamID = reader.GetInt16("killed_team_id");
            ev.KilledVehicleID = reader.GetString("killed_vehicle_id");
            ev.FacilityID = reader.GetString("facility_id");
            ev.WorldID = reader.GetInt16("world_id");
            ev.ZoneID = reader.GetUInt32("zone_id");
            ev.Timestamp = reader.GetDateTime("timestamp");

            return ev;
        }
    }
}
