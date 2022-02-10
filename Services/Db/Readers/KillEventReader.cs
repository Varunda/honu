using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Readers {

    public class KillEventReader : IDataReader<KillEvent> {

        public override KillEvent ReadEntry(NpgsqlDataReader reader) {
            KillEvent ev = new KillEvent();

            ev.AttackerCharacterID = reader.GetString("attacker_character_id");
            ev.AttackerLoadoutID = reader.GetInt16("attacker_loadout_id");
            ev.AttackerTeamID = reader.GetInt16("attacker_team_id");
            ev.AttackerFireModeID = reader.GetInt32("attacker_fire_mode_id");
            ev.AttackerVehicleID = reader.GetInt32("attacker_vehicle_id");

            ev.KilledCharacterID = reader.GetString("killed_character_id");
            ev.KilledLoadoutID = reader.GetInt16("killed_loadout_id");
            ev.KilledTeamID = reader.GetInt16("killed_team_id");

            ev.Timestamp = reader.GetDateTime("timestamp");
            ev.IsHeadshot = reader.GetBoolean("is_headshot");
            ev.RevivedEventID = reader.GetNullableInt64("revived_event_id");
            ev.WeaponID = reader.GetInt32("weapon_id");
            ev.ZoneID = reader.GetUInt32("zone_id");
            ev.WorldID = reader.GetInt16("world_id");

            return ev;
        }

    }
}
