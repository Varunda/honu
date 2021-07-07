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
using watchtower.Models.Events;

namespace watchtower.Services.Db.Implementations {

    public class KillEventDbStore : IKillEventDbStore {

        private readonly ILogger<KillEventDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<KillDbEntry> _KillDbReader;
        private readonly IDataReader<KillDbOutfitEntry> _KillOutfitReader;

        public KillEventDbStore(ILogger<KillEventDbStore> logger,
            IDbHelper dbHelper, IDataReader<KillDbEntry> killDbReader,
            IDataReader<KillDbOutfitEntry> outfitDbReader) {

            _Logger = logger;

            _DbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));

            _KillDbReader = killDbReader ?? throw new ArgumentNullException(nameof(killDbReader));
            _KillOutfitReader = outfitDbReader ?? throw new ArgumentNullException(nameof(outfitDbReader));
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
            cmd.AddParameter("AttackerTeamID", ev.AttackerTeamID);
            cmd.AddParameter("KilledCharacterID", ev.KilledCharacterID);
            cmd.AddParameter("KilledLoadoutID", ev.KilledLoadoutID);
            cmd.AddParameter("KilledFactionID", Loadout.GetFaction(ev.KilledLoadoutID));
            cmd.AddParameter("KilledTeamID", ev.KilledTeamID);
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

        public async Task<List<KillDbEntry>> GetTopKillers(KillDbOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH top_killers AS (
                    SELECT attacker_character_id
                        FROM wt_kills
                        WHERE (timestamp + (@Interval || ' minutes')::INTERVAL) >= NOW() at time zone 'utc'
                            AND world_id = @WorldID
                            AND attacker_team_id = @FactionID
                        GROUP BY attacker_character_id
                        ORDER BY count(attacker_character_id) DESC
                        LIMIT 8
                ), evs AS (
                    SELECT ID, attacker_character_id, killed_character_id, revived_event_id
                        FROM wt_kills
                        WHERE (timestamp + (@Interval || ' minutes')::INTERVAL) >= NOW() at time zone 'utc'
                            AND world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                ), exp as (
                    SELECT id, source_character_id
                        FROM wt_exp 
                        WHERE (timestamp + interval '120 minutes') >= NOW() at time zone 'utc'
                            AND world_id = 1
                            AND source_team_id = 1
                            AND (experience_id = 2 OR experience_id = 3 OR experience_id = 371 OR experience_id = 372)
                            AND source_character_id IN (SELECT attacker_character_id FROM top_killers)
                )
                SELECT
                    attacker_character_id,
                    (SELECT COUNT(*) FROM evs k WHERE k.attacker_character_id = top_killers.attacker_character_id) AS kills,
                    (SELECT COUNT(*) FROM evs d WHERE d.killed_character_id = top_killers.attacker_character_id AND revived_event_id IS null) AS deaths,
                    (SELECT COUNT(*) FROM exp e WHERE e.source_character_id = top_killers.attacker_character_id) AS assists
                FROM
                    top_killers
                GROUP BY attacker_character_id
                ORDER BY kills DESC;
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("FactionID", options.FactionID);

            List<KillDbEntry> entries = await _KillDbReader.ReadList(cmd);

            return entries;
        }

        public async Task<List<KillDbOutfitEntry>> GetTopOutfitKillers(KillDbOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT k.id, k.world_id, k.zone_id, c.outfit_id AS attacker_outfit_id, c2.outfit_id AS killed_outfit_id, revived_event_id, k.attacker_character_id, k.killed_character_id
                        FROM wt_kills k
                            INNER JOIN wt_character c on k.attacker_character_id = c.id
                            INNER JOIN wt_character c2 on k.killed_character_id = c2.id
                        WHERE (k.timestamp + (@Interval || ' minutes')::INTERVAL) >= NOW() at time zone 'utc'
                            AND k.world_id = @WorldID
                            AND attacker_faction_id != killed_faction_id
                            AND (attacker_faction_id = @FactionID OR killed_faction_id = @FactionID)
                            AND c.outfit_id IS NOT NULL
                ), outfits AS (
                    SELECT attacker_outfit_id
                        FROM evs
                        GROUP BY attacker_outfit_id
                )
                SELECT attacker_outfit_id AS outfit_id,
                    (SELECT COUNT(*) FROM evs k WHERE k.attacker_outfit_id = outfits.attacker_outfit_id) AS kills,
                    (SELECT COUNT(*) FROM evs d WHERE d.killed_outfit_id = outfits.attacker_outfit_id AND d.revived_event_id IS NULL) AS deaths,
                    (SELECT COUNT(DISTINCT(attacker_character_id)) FROM evs c WHERE c.attacker_outfit_id = outfits.attacker_outfit_id) AS members
                    FROM outfits
                        JOIN wt_outfit ON outfits.attacker_outfit_id = wt_outfit.id
                    WHERE wt_outfit.faction_id = @FactionID
                    ORDER BY kills DESC;
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("FactionID", options.FactionID);

            List<KillDbOutfitEntry> entries = await _KillOutfitReader.ReadList(cmd);

            return entries;
        }

    }
}
