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

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the kill table in the DB
    /// </summary>
    public class KillEventDbStore {

        private readonly ILogger<KillEventDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<KillDbEntry> _KillDbReader;
        private readonly IDataReader<KillDbOutfitEntry> _KillOutfitReader;
        private readonly IDataReader<KillEvent> _KillEventReader;
        private readonly IDataReader<KillItemEntry> _KillItemEntryReader;

        public KillEventDbStore(ILogger<KillEventDbStore> logger,
            IDbHelper dbHelper, IDataReader<KillDbEntry> killDbReader,
            IDataReader<KillDbOutfitEntry> outfitDbReader, IDataReader<KillEvent> evReader,
            IDataReader<KillItemEntry> itemReader) {

            _Logger = logger;

            _DbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));

            _KillDbReader = killDbReader ?? throw new ArgumentNullException(nameof(killDbReader));
            _KillOutfitReader = outfitDbReader ?? throw new ArgumentNullException(nameof(outfitDbReader));
            _KillEventReader = evReader ?? throw new ArgumentNullException(nameof(evReader));
            _KillItemEntryReader = itemReader ?? throw new ArgumentNullException(nameof(itemReader));
        }

        /// <summary>
        ///     Insert a new <see cref="KillEvent"/>
        /// </summary>
        /// <param name="ev">Event to be inserted</param>
        /// <returns>
        ///     The ID of the <see cref="KillEvent"/> that was just created
        /// </returns>
        public async Task<long> Insert(KillEvent ev) {
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
                ) RETURNING id;
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

            //_Logger.LogTrace($"{ev.Timestamp.Kind} {ev.Timestamp}");

            object? objID = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();

            if (objID != null && int.TryParse(objID.ToString(), out int ID) == true) {
                return ID;
            } else {
                throw new Exception($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        /// <summary>
        ///     Update <see cref="KillEvent.RevivedEventID"/>
        /// </summary>
        /// <param name="charID">Character ID that was revived</param>
        /// <param name="revivedEventID">ID of the exp event that the revive came from</param>
        public async Task SetRevivedID(string charID, long revivedEventID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_kills
                    SET revived_event_id = @RevivedEventID
                    WHERE killed_character_id = @RevivedCharacterID
                        AND timestamp = (
                            SELECT MAX(timestamp)
                                FROM wt_kills 
                                WHERE timestamp >= (NOW() at time zone 'utc' - interval '50 seconds')
                                    AND killed_character_id = @RevivedCharacterID
                        )
            ");

            cmd.AddParameter("RevivedCharacterID", charID);
            cmd.AddParameter("RevivedEventID", revivedEventID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Update the <see cref="KillEvent.KilledTeamID"/> of a kill event
        /// </summary>
        /// <param name="eventID">ID of the kill event</param>
        /// <param name="newTeamID"></param>
        /// <returns></returns>
        public async Task UpdateKilledTeamID(long eventID, short newTeamID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_kills
                    SET killed_team_id = @TeamID
                    WHERE ID = @ID;
            ");

            cmd.AddParameter("ID", eventID);
            cmd.AddParameter("TeamID", newTeamID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get the top 8 killers from the parameters given
        /// </summary>
        /// <param name="options">Options used to generate the data</param>
        /// <returns></returns>
        public async Task<List<KillDbEntry>> GetTopKillers(KillDbOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH top_killers AS (
                    SELECT attacker_character_id
                        FROM wt_kills
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND attacker_team_id = @FactionID
                            AND attacker_team_id != killed_team_id
                        GROUP BY attacker_character_id
                        ORDER BY count(attacker_character_id) DESC
                        LIMIT 8
                ), evs AS (
                    SELECT ID, attacker_character_id, killed_character_id, revived_event_id, attacker_team_id, killed_team_id
                        FROM wt_kills
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                ), exp as (
                    SELECT id, source_character_id
                        FROM wt_exp 
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND source_team_id = @FactionID
                            AND (experience_id = 2 OR experience_id = 3 OR experience_id = 371 OR experience_id = 372)
                            AND source_character_id IN (SELECT attacker_character_id FROM top_killers)
                )
                SELECT
                    attacker_character_id,
                    (SELECT COUNT(*) FROM evs k WHERE k.attacker_character_id = top_killers.attacker_character_id AND k.attacker_team_id = @FactionID) AS kills,
                    (SELECT COUNT(*) FROM evs d WHERE d.killed_character_id = top_killers.attacker_character_id AND d.attacker_team_id != @FactionID AND revived_event_id IS null) AS deaths,
                    (SELECT COUNT(*) FROM exp e WHERE e.source_character_id = top_killers.attacker_character_id) AS assists,
                    (SELECT LEAST(@Interval * 60, 
                            EXTRACT('epoch' FROM SUM(COALESCE(s.finish, NOW() at time zone 'utc')
                            - GREATEST(NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL, s.start))
                        ))
                        FROM wt_session s 
                        WHERE s.character_id = top_killers.attacker_character_id
                            AND (s.team_id = @FactionID OR s.team_id = 4)
                            AND (s.finish IS NULL OR s.finish >= NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                    ) AS seconds_online
                FROM
                    top_killers
                GROUP BY attacker_character_id
                ORDER BY kills DESC;
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("FactionID", options.FactionID);

            List<KillDbEntry> entries = await _KillDbReader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Get the top killers in an outfit
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<List<KillDbOutfitEntry>> GetTopOutfitKillers(KillDbOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT k.id, k.world_id, k.zone_id, c.outfit_id AS attacker_outfit_id, c2.outfit_id AS killed_outfit_id, revived_event_id, k.attacker_character_id, k.killed_character_id
                        FROM wt_kills k
                            INNER JOIN wt_character c on k.attacker_character_id = c.id
                            INNER JOIN wt_character c2 on k.killed_character_id = c2.id
                        WHERE k.timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND k.world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                            AND (attacker_team_id = @FactionID OR killed_team_id = @FactionID)
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
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Get the top weapons used 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<List<KillItemEntry>> GetTopWeapons(KillDbOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT 
                    weapon_id as item_id,
                    COUNT(weapon_id) AS kills,
                    COUNT(weapon_id) FILTER (WHERE is_headshot = true) AS headshots,
                    COUNT(DISTINCT attacker_character_id) AS users
                FROM wt_kills
                WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                    AND world_id = @WorldID
                    AND attacker_team_id = @FactionID
                    AND attacker_team_id != killed_team_id
                GROUP BY weapon_id
                ORDER BY COUNT(*) DESC;
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("FactionID", options.FactionID);

            List<KillItemEntry> entries = await _KillItemEntryReader.ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Get all the kills and deaths of a character between a period
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<KillEvent>> GetKillsByCharacterID(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_kills
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND (attacker_character_id = @CharacterID OR killed_character_id = @CharacterID);
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get all kills and deaths of a set of characters
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<KillEvent>> GetKillsByCharacterIDs(List<string> IDs, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_kills
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND (attacker_character_id = ANY(@IDs) OR killed_character_id = ANY(@IDs))
            ");

            cmd.AddParameter("IDs", IDs);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get the 
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task<List<KillEvent>> GetRecentKillsByCharacterID(string charID, int interval) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_kills
                    WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                        AND attacker_character_id = @CharacterID
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("Interval", interval);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get all kills of an outfit
        /// </summary>
        /// <param name="outfitID"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task<List<KillEvent>> GetKillsByOutfitID(string outfitID, int interval) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT wt_kills.*
                    FROM wt_kills
                        INNER JOIN wt_character ON wt_kills.attacker_character_id = wt_character.id
                    WHERE wt_kills.timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                        AND wt_character.outfit_id = @OutfitID
            ");

            cmd.AddParameter("OutfitID", outfitID);
            cmd.AddParameter("Interval", interval);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get the kills that occured between a time period, optionally limiting to a zone and world
        /// </summary>
        /// <param name="start">Start period</param>
        /// <param name="end">End period</param>
        /// <param name="zoneID">Optional, zone ID to limit the kills by</param>
        /// <param name="worldID">Optional, world ID to limit the kills by</param>
        /// <returns>
        ///     All <see cref="KillEvent"/>s that occured between the range given. If <paramref name="zoneID"/>
        ///     and/or <paramref name="worldID"/> is given, the event will match those options given
        /// </returns>
        public async Task<List<KillEvent>> GetByRange(DateTime start, DateTime end, uint? zoneID, short? worldID) {
            if (end <= start) {
                throw new ArgumentException($"{nameof(start)} {start:u} must come before {nameof(end)} {end:u}");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    FROM wt_kills
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                    {(zoneID != null ? " AND zone_id = @ZoneID " : "")}
                    {(worldID != null ? " AND world_id = @WorldID " : "")}
            ");

            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);
            cmd.AddParameter("ZoneID", zoneID);
            cmd.AddParameter("WorldID", worldID);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

    }

}
