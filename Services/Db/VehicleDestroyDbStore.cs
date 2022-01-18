using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the vehicle_destroy table
    /// </summary>
    public class VehicleDestroyDbStore {

        private readonly ILogger<VehicleDestroyDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<VehicleDestroyEvent> _Reader;
        
        public VehicleDestroyDbStore(ILogger<VehicleDestroyDbStore> logger,
            IDbHelper helper, IDataReader<VehicleDestroyEvent> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Insert a new <see cref="VehicleDestroyEvent"/> into the storing Db
        /// </summary>
        /// <param name="ev">Event to be inserted</param>
        /// <param name="cancel">Cancellation token</param>
        public async Task<long> Insert(VehicleDestroyEvent ev, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO vehicle_destroy (
                    attacker_character_id, attacker_vehicle_id, attacker_weapon_id, attacker_loadout_id, attacker_team_id, attacker_faction_id,
                    killed_character_id, killed_faction_id, killed_team_id, killed_vehicle_id,
                    facility_id, world_id, zone_id, timestamp
                ) VALUES (
                    @AttackerCharacterID, @AttackerVehicleID, @AttackerWeaponID, @AttackerLoadoutID, @AttackerTeamID, @AttackerFactionID,
                    @KilledCharacterID, @KilledFactionID, @KilledTeamID, @KilledVehicleID,
                    @FacilityID, @WorldID, @ZoneID, @Timestamp
                ) RETURNING id;
            ");

            cmd.AddParameter("AttackerCharacterID", ev.AttackerCharacterID);
            cmd.AddParameter("AttackerVehicleID", ev.AttackerVehicleID);
            cmd.AddParameter("AttackerWeaponID", ev.AttackerWeaponID);
            cmd.AddParameter("AttackerLoadoutID", ev.AttackerLoadoutID);
            cmd.AddParameter("AttackerTeamID", ev.AttackerTeamID);
            cmd.AddParameter("AttackerFactionID", ev.AttackerFactionID);
            cmd.AddParameter("KilledCharacterID", ev.KilledCharacterID);
            cmd.AddParameter("KilledFactionID", ev.KilledFactionID);
            cmd.AddParameter("KilledTeamID", ev.KilledTeamID);
            cmd.AddParameter("KilledVehicleID", ev.KilledVehicleID);
            cmd.AddParameter("FacilityID", ev.FacilityID);
            cmd.AddParameter("WorldID", ev.WorldID);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("Timestamp", ev.Timestamp);

            return await cmd.ExecuteInt64(cancel);
        }

    }
}
