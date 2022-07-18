using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class WorldTotalDbStore : IDataReader<WorldTotalEntry>, IWorldTotalDbStore {

        private readonly ILogger<WorldTotalDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public WorldTotalDbStore(ILogger<WorldTotalDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<WorldTotal> Get(WorldTotalOptions options) {
            string isHeal = $"experience_id = {Experience.HEAL} OR experience_id = {Experience.SQUAD_HEAL}";
            string isRevive = $"experience_id = {Experience.REVIVE} OR experience_id = {Experience.SQUAD_REVIVE}";
            string isResupply = $"experience_id = {Experience.RESUPPLY} OR experience_id = {Experience.SQUAD_RESUPPLY}";
            string isShield = $"experience_id = {Experience.SHIELD_REPAIR} OR experience_id = {Experience.SQUAD_SHIELD_REPAIR}";

            string isSpawn = $"experience_id = {Experience.SQUAD_SPAWN} OR experience_id = {Experience.GALAXY_SPAWN_BONUS} "
                + $"OR experience_id = {Experience.GENERIC_NPC_SPAWN} OR experience_id = {Experience.SQUAD_VEHICLE_SPAWN_BONUS} "
                + $"OR experience_id = {Experience.SUNDERER_SPAWN_BONUS}";

            string isAssist = $"experience_id = {Experience.ASSIST} OR experience_id = {Experience.SPAWN_ASSIST} "
                + $"OR experience_id = {Experience.PRIORITY_ASSIST} OR experience_id = {Experience.HIGH_PRIORITY_ASSIST}";

            string isVehicleKill = $"experience_id = {Experience.VKILL_FLASH} OR experience_id = {Experience.VKILL_GALAXY} "
                + $"OR experience_id = {Experience.VKILL_HARASSER} OR experience_id = {Experience.VKILL_JAVELIN} "
                + $"OR experience_id = {Experience.VKILL_LIBERATOR} OR experience_id = {Experience.VKILL_LIGHTNING} "
                + $"OR experience_id = {Experience.VKILL_MAGRIDER} OR experience_id = {Experience.VKILL_MOSQUITO} "
                + $"OR experience_id = {Experience.VKILL_PROWLER} OR experience_id = {Experience.VKILL_REAVER} "
                + $"OR experience_id = {Experience.VKILL_SCYTHE} OR experience_id = {Experience.VKILL_VALKYRIE} "
                + $"OR experience_id = {Experience.VKILL_VANGUARD} OR experience_id = {Experience.VKILL_DERVISH} "
                + $"OR experience_id = {Experience.VKILL_CHIMERA} ";

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                WITH kills AS (
                    SELECT *
                        FROM wt_recent_kills
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                ), exp AS (
                    SELECT *
                        FROM wt_recent_exp
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                )
                SELECT 'vs_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1) AS value
                UNION SELECT 'vs_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 1 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'vs_kills_nc' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1 AND kills.killed_team_id = 2 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'vs_kills_tr' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1 AND kills.killed_team_id = 3 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'vs_assists' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isAssist})) AS value

                UNION SELECT 'vs_heals' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isHeal})) AS value
                UNION SELECT 'vs_revives' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isRevive})) AS value
                UNION SELECT 'vs_resupplies' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isResupply})) AS value
                UNION SELECT 'vs_spawns' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isSpawn})) AS value
                UNION SELECT 'vs_vehicle_kills' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isVehicleKill})) AS value
                UNION SELECT 'vs_shield_repair' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 1 AND ({isShield})) AS value

                UNION SELECT 'nc_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2) AS value
                UNION SELECT 'nc_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 2 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'nc_kills_vs' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2 AND kills.killed_team_id = 1 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'nc_kills_tr' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2 AND kills.killed_team_id = 3 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'nc_assists' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isAssist})) AS value

                UNION SELECT 'nc_heals' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isHeal})) AS value
                UNION SELECT 'nc_revives' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isRevive})) AS value
                UNION SELECT 'nc_resupplies' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isResupply})) AS value
                UNION SELECT 'nc_spawns' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isSpawn})) AS value
                UNION SELECT 'nc_vehicle_kills' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isVehicleKill})) AS value
                UNION SELECT 'nc_shield_repair' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 2 AND ({isShield})) AS value

                UNION SELECT 'tr_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3) AS value
                UNION SELECT 'tr_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 3 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'tr_kills_vs' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3 AND kills.killed_team_id = 1 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'tr_kills_tr' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3 AND kills.killed_team_id = 2 AND kills.revived_event_id IS NULL) AS value
                UNION SELECT 'tr_assists' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isAssist})) AS value

                UNION SELECT 'tr_heals' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isHeal})) AS value
                UNION SELECT 'tr_revives' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isRevive})) AS value
                UNION SELECT 'tr_resupplies' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isResupply})) AS value
                UNION SELECT 'tr_spawns' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isSpawn})) AS value
                UNION SELECT 'tr_vehicle_kills' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isVehicleKill})) AS value
                UNION SELECT 'tr_shield_repair' AS key, (SELECT COUNT(*) FROM exp WHERE exp.source_team_id = 3 AND ({isShield})) AS value
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
	        await cmd.PrepareAsync();

            //_Logger.LogDebug($"TEXT:\n{cmd.CommandText}");

            WorldTotal total = new WorldTotal();
            total.Entries = await ReadList(cmd);
            await conn.CloseAsync();

            return total;
        }

        public async Task<WorldTotal> GetFocus(WorldTotalOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                WITH kills AS (
                    SELECT *
                        FROM wt_recent_kills
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                )
                SELECT 'vs_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1) AS value
                UNION SELECT 'vs_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 1 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'vs_kills_nc' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1 AND kills.killed_team_id = 2) AS value
                UNION SELECT 'vs_kills_tr' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 1 AND kills.killed_team_id = 3) AS value

                UNION SELECT 'nc_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2) AS value
                UNION SELECT 'nc_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 2 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'nc_kills_vs' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2 AND kills.killed_team_id = 1) AS value
                UNION SELECT 'nc_kills_tr' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 2 AND kills.killed_team_id = 3) AS value

                UNION SELECT 'tr_kills' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3) AS value
                UNION SELECT 'tr_deaths' AS key, (SELECT COUNT(*) FROM kills WHERE kills.killed_team_id = 3 AND kills.revived_event_id IS null) AS value
                UNION SELECT 'tr_kills_vs' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3 AND kills.killed_team_id = 1) AS value
                UNION SELECT 'tr_kills_nc' AS key, (SELECT COUNT(*) FROM kills WHERE kills.attacker_team_id = 3 AND kills.killed_team_id = 2) AS value
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
	        await cmd.PrepareAsync();

            //_Logger.LogDebug($"TEXT:\n{cmd.CommandText}");

            WorldTotal total = new WorldTotal();
            total.Entries = await ReadList(cmd);
            await conn.CloseAsync();

            return total;
        }

        public override WorldTotalEntry ReadEntry(NpgsqlDataReader reader) {
            WorldTotalEntry entry = new WorldTotalEntry();

            entry.Key = reader.GetString("key");
            entry.Value = reader.GetInt32("value");

            return entry;
        }

    }
}
