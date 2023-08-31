using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
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
        ///     Get the vehicle destroy events that a character got during a time period
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="start">Lower bound of the range</param>
        /// <param name="end">Upper bound of the range</param>
        /// <returns></returns>
        public async Task<List<VehicleDestroyEvent>> GetByCharacterID(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM vehicle_destroy
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND (attacker_character_id = @CharacterID OR killed_character_id = @CharacterID)
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<VehicleDestroyEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get the vehicle destroy events a block of characters got
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<VehicleDestroyEvent>> GetByCharacterIDs(List<string> IDs, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM vehicle_destroy
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND (attacker_character_id = ANY(@CharacterIDs) OR killed_character_id = ANY(@CharacterIDs))
            ");

            cmd.AddParameter("CharacterID", IDs);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<VehicleDestroyEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;

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

        public async Task<List<VehicleDestroyEvent>> LoadWrappedKills(string charID, DateTime year) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    from vehicle_destroy_kill_{year:yyyy}
                    WHERE attacker_character_id = @CharID;
            ");

            cmd.AddParameter("CharID", charID);

            List<VehicleDestroyEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        public async Task<List<VehicleDestroyEvent>> LoadWrappedDeaths(string charID, DateTime year) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    from vehicle_destroy_death_{year:yyyy}
                    WHERE killed_character_id = @CharID;
            ");

            cmd.AddParameter("CharID", charID);

            List<VehicleDestroyEvent> evs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

    }
}
