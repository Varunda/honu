using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Events;

namespace watchtower.Services.Db.Implementations {

    public class KillEventDbStore : IKillEventDbStore {

        private readonly ILogger<KillEventDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public KillEventDbStore(ILogger<KillEventDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public async Task Insert(KillEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_kills (
                    world_id, zone_id,
                    attacker_character_id, attacker_loadout_id, attacker_fire_mode_id, attacker_vehicle_id, attacker_faction_id, attacker_team_id,
                    killed_character_id, killed_loadout_id, killed_faction_id, killed_team_id, revived_event_id,
                    weapon_id, is_headshot, timestamp
                ) VALUES (
                    @WorldID, @ZoneID,
                    @AttackerCharacterID, @AttackerLoadoutID, @AttackerFireModeID, @AttackerVehicleID, @AttackerFactionID, @AttackerTeamID,
                    @KilledCharacterID, @KilledLoadoutID, @KilledFactionID, @KilledTeamID, @RevivedEventID,
                    @WeaponID, @IsHeadshot, @Timestamp
                );
            ");

            cmd.AddParameter("WorldID", ev.WorldID);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("AttackerCharacterID", ev.AttackerCharacterID);
            cmd.AddParameter("AttackerLoadoutID", ev.AttackerLoadoutID);
            cmd.AddParameter("AttackerFireModeID", ev.AttackerFireModeID);
            cmd.AddParameter("AttackerVehicleID", ev.AttackerVehicleID);
            cmd.AddParameter("AttackerFactionID", Loadout.GetFaction(ev.AttackerLoadoutID));
            cmd.AddParameter("AttackerTeamID", Loadout.GetFaction(ev.AttackerLoadoutID));
            cmd.AddParameter("KilledCharacterID", ev.KilledCharacterID);
            cmd.AddParameter("KilledLoadoutID", ev.KilledLoadoutID);
            cmd.AddParameter("KilledFactionID", Loadout.GetFaction(ev.KilledLoadoutID));
            cmd.AddParameter("KilledTeamID", Loadout.GetFaction(ev.KilledLoadoutID));
            cmd.AddParameter("RevivedEventID", null);
            cmd.AddParameter("WeaponID", ev.WeaponID);
            cmd.AddParameter("IsHeadshot", ev.IsHeadshot);
            cmd.AddParameter("Timestamp", ev.Timestamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetRevivedID(string charID, long revivedID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_kills
                    SET revived_event_id = @RevivedID
                    WHERE killed_character_id = @RevivedCharacterID
                        AND timestamp = (SELECT MAX(timestamp) FROM wt_kills WHERE killed_character_id = @RevivedCharacterID);
            ");

            cmd.AddParameter("RevivedID", revivedID);
            cmd.AddParameter("RevivedCharacterID", charID);

            await cmd.ExecuteNonQueryAsync();
        }

    }
}
