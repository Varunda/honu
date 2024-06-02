using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Constants;
using watchtower.Models.Census;
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
            // insert into EVENTS db
            await using NpgsqlConnection conn = _DbHelper.Connection(task: "kill insert", enlist: false);
            await conn.OpenAsync();

            await using NpgsqlCommand cmd = new NpgsqlCommand(@"
                INSERT INTO wt_kills (
                    world_id, zone_id,
                    attacker_character_id, attacker_loadout_id,
                    attacker_fire_mode_id, attacker_vehicle_id,
                    attacker_faction_id, attacker_team_id,
                    killed_character_id, killed_loadout_id, killed_faction_id, killed_team_id, revived_event_id,
                    weapon_id, is_headshot, timestamp
                ) VALUES (
                    $1, $2,
                    $3, $4,
                    $5, $6,
                    $7, $8,
                    $9, $10, $11, $12, null,
                    $13, $14, $15
                ) RETURNING id;
            ", conn) {
                Parameters = {
                    new() { Value = ev.WorldID }, new() { Value = unchecked((int)ev.ZoneID) },
                    new() { Value = ev.AttackerCharacterID }, new() { Value = ev.AttackerLoadoutID },
                    new() { Value = ev.AttackerFireModeID }, new() { Value = ev.AttackerVehicleID },
                    new() { Value = Loadout.GetFaction(ev.AttackerLoadoutID) }, new() { Value = ev.AttackerTeamID },
                    new() { Value = ev.KilledCharacterID },
                    new() { Value = ev.KilledLoadoutID },
                    new() { Value = Loadout.GetFaction(ev.KilledLoadoutID) },
                    new() { Value = ev.KilledTeamID },
                    new() { Value = ev.WeaponID },
                    new() { Value = ev.IsHeadshot },
                    new() { Value = ev.Timestamp }
                }
            };

            await cmd.PrepareAsync();

            //Activity? allExe = HonuActivitySource.Root.StartActivity("insert into wt_kills");
            object? IDobj = await cmd.ExecuteScalarAsync();
            //allExe?.Stop();
            if (IDobj == null) {
                throw new NullReferenceException($"The scalar returned when inserting a kill was null");
            }

            long ID = (long)IDobj;

            // insert into CHARACTER db, which has the recent kills
            await using NpgsqlConnection charConn = _DbHelper.Connection(Dbs.CHARACTER, task: "kill insert", enlist: false);
            await charConn.OpenAsync();

            using NpgsqlCommand charCmd = new NpgsqlCommand(@"
                INSERT INTO wt_recent_kills (
                    id, world_id, zone_id,
                    attacker_character_id, attacker_loadout_id,
                    attacker_fire_mode_id, attacker_vehicle_id,
                    attacker_faction_id, attacker_team_id,
                    killed_character_id, killed_loadout_id, killed_faction_id, killed_team_id, revived_event_id,
                    weapon_id, is_headshot, timestamp
                ) VALUES (
                    $16, $1, $2,
                    $3, $4,
                    $5, $6,
                    $7, $8,
                    $9, $10, $11, $12, null,
                    $13, $14, $15
                ) RETURNING id;
            ", charConn) {
                Parameters = {
                    new() { Value = ev.WorldID }, new() { Value = unchecked((int)ev.ZoneID) },
                    new() { Value = ev.AttackerCharacterID }, new() { Value = ev.AttackerLoadoutID },
                    new() { Value = ev.AttackerFireModeID }, new() { Value = ev.AttackerVehicleID },
                    new() { Value = Loadout.GetFaction(ev.AttackerLoadoutID) }, new() { Value = ev.AttackerTeamID },
                    new() { Value = ev.KilledCharacterID },
                    new() { Value = ev.KilledLoadoutID },
                    new() { Value = Loadout.GetFaction(ev.KilledLoadoutID) },
                    new() { Value = ev.KilledTeamID },
                    new() { Value = ev.WeaponID },
                    new() { Value = ev.IsHeadshot },
                    new() { Value = ev.Timestamp },
                    new() { Value = ID } // <==== ID IS LAST, $16!!!!!
                }
            };

            await charCmd.PrepareAsync();

            //Activity? recentExe = HonuActivitySource.Root.StartActivity("insert into wt_recent_kills");
            await charCmd.ExecuteNonQueryAsync();
            //recentExe?.Stop();

            await Task.WhenAll(conn.CloseAsync(), charConn.CloseAsync());

            return ID;
        }

        /// <summary>
        ///     Set the <see cref="KillEvent.RevivedEventID"/>
        /// </summary>
        /// <param name="deathID">ID of the <see cref="KillEvent"/></param>
        /// <param name="expID">ID of the <see cref="ExpEvent"/> that was the revive</param>
        /// <returns></returns>
        public async Task SetRevived(long deathID, long expID) {
            using NpgsqlConnection evConn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand evCmd = await _DbHelper.Command(evConn, @"
                UPDATE wt_kills
                    SET revived_event_id = @ExpID
                    WHERE id = @DeathID;
            ");
            evCmd.AddParameter("ExpID", expID);
            evCmd.AddParameter("DeathID", deathID);
            await evCmd.PrepareAsync();

            using NpgsqlConnection charConn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand charCmd = await _DbHelper.Command(charConn, @"
                UPDATE wt_recent_kills
                    SET revived_event_id = @ExpID
                    WHERE id = @DeathID;
            ");
            charCmd.AddParameter("ExpID", expID);
            charCmd.AddParameter("DeathID", deathID);
            await charCmd.PrepareAsync();

            await Task.WhenAll(evCmd.ExecuteNonQueryAsync(), charCmd.ExecuteNonQueryAsync());

            await Task.WhenAll(evConn.CloseAsync(), charConn.CloseAsync());
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
            using Activity? trace = HonuActivitySource.Root.StartActivity("get top killers");
            trace?.AddTag("honu.worldID", options.WorldID);
            trace?.AddTag("honu.factionID", options.FactionID);
            trace?.AddTag("honu.duration", options.Interval);

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT ID, attacker_character_id, killed_character_id, revived_event_id, attacker_team_id, killed_team_id
                        FROM wt_recent_kills
                        WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND world_id = @WorldID
                            AND attacker_team_id != killed_team_id
                ), top_killers AS (
                    SELECT attacker_character_id
                        FROM evs
                        WHERE attacker_team_id = @FactionID
                        GROUP BY attacker_character_id
                        ORDER BY count(attacker_character_id) DESC
                        LIMIT 8
                ), exp as (
                    SELECT id, source_character_id
                        FROM wt_recent_exp 
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
            using Activity? trace = HonuActivitySource.Root.StartActivity("get top outift killers");
            trace?.AddTag("honu.worldID", options.WorldID);
            trace?.AddTag("honu.factionID", options.FactionID);
            trace?.AddTag("honu.duration", options.Interval);

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT k.id, k.world_id, k.zone_id, c.outfit_id AS attacker_outfit_id, c2.outfit_id AS killed_outfit_id, revived_event_id, k.attacker_character_id, k.killed_character_id
                        FROM wt_recent_kills k
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
                    WHERE 
                        (wt_outfit.faction_id = @FactionID OR wt_outfit.faction_id = 4 OR wt_outfit.faction_id = -1)
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
            using Activity? trace = HonuActivitySource.Root.StartActivity("get top weapons");
            trace?.AddTag("honu.worldID", options.WorldID);
            trace?.AddTag("honu.factionID", options.FactionID);
            trace?.AddTag("honu.duration", options.Interval);

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT 
                    weapon_id as item_id,
                    COUNT(weapon_id) AS kills,
                    COUNT(weapon_id) FILTER (WHERE is_headshot = true) AS headshots,
                    COUNT(DISTINCT attacker_character_id) AS users
                FROM wt_recent_kills
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
        /// <param name="charID">ID of the character</param>
        /// <param name="start">When the start period is</param>
        /// <param name="end">When the end period is</param>
        /// <returns>
        ///     A list <see cref="KillEvent"/>s that have <paramref name="charID"/> as either
        ///     <see cref="KillEvent.AttackerCharacterID"/> or <see cref="KillEvent.KilledCharacterID"/>
        /// </returns>
        public async Task<List<KillEvent>> GetKillsByCharacterID(string charID, DateTime start, DateTime end) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("kills by character id");
            trace?.AddTag("honu.characterID", charID);
            trace?.AddTag("honu.start", $"{start:u}");
            trace?.AddTag("honu.end", $"{end:u}");

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
            using Activity? trace = HonuActivitySource.Root.StartActivity("kills by character ids");
            trace?.AddTag("honu.characterID", IDs);
            trace?.AddTag("honu.start", $"{start:u}");
            trace?.AddTag("honu.end", $"{end:u}");

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
        ///     Get all kills of an outfit based on who is currently within the outfit
        /// </summary>
        /// <param name="outfitID">ID of outfit</param>
        /// <param name="interval">How many minutes back to go</param>
        /// <returns>
        ///     A list of all <see cref="KillEvent"/>s characters within the <see cref="PsOutfit.ID"/>
        ///     of <paramref name="outfitID"/> within the last <paramref name="interval"/> minutes
        /// </returns>
        public async Task<List<KillEvent>> GetKillsByOutfitID(string outfitID, int interval) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT wt_recent_kills.*
                    FROM wt_recent_kills
                        INNER JOIN wt_character ON wt_recent_kills.attacker_character_id = wt_character.id
                    WHERE wt_recent_kills.timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
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
        public async Task<List<KillEvent>> GetByRange(DateTime start, DateTime end, uint? zoneID = null, short? worldID = null) {
            if (end <= start) {
                throw new ArgumentException($"{nameof(start)} {start:u} must come before {nameof(end)} {end:u}");
            }

            bool useAll = (DateTime.UtcNow - end) > TimeSpan.FromHours(2) || (DateTime.UtcNow - start) > TimeSpan.FromHours(2);

            using NpgsqlConnection conn = _DbHelper.Connection(useAll == true ? Dbs.EVENTS : Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    FROM {(useAll ? "wt_kills" : "wt_recent_kills")}
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                    {(zoneID != null ? " AND zone_id = @ZoneID " : "")}
                    {(worldID != null ? " AND world_id = @WorldID " : "")}
            ");
            cmd.CommandTimeout = 300;

            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);
            cmd.AddParameter("ZoneID", zoneID);
            cmd.AddParameter("WorldID", worldID);

            await cmd.PrepareAsync();

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        /// <summary>
        ///     Get the wrapped kills of a character for a single year
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="year">Year of the data of. Only the year part is used</param>
        /// <returns>
        ///     A list of <see cref="KillEvent"/> that occured within the year of <paramref name="charID"/>
        ///     with a <see cref="KillEvent.AttackerCharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<KillEvent>> LoadWrappedKills(string charID, DateTime year) {
            string db = $"wrapped_{year:yyyy}";

            using NpgsqlConnection conn = _DbHelper.Connection(db);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    from wt_kills_{year:yyyy}
                    WHERE attacker_character_id = @CharID;
            ");

            cmd.AddParameter("CharID", charID);

            List<KillEvent> kills = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return kills;
        }

        /// <summary>
        ///     Get the wrapped deaths of a character for a single year
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="year">Year to get the data of. Only the year part is used</param>
        /// <returns>
        ///     A list of <see cref="KillEvent"/>s that occured during the year of <paramref name="year"/>
        ///     with a <see cref="KillEvent.KilledCharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<KillEvent>> LoadWrappedDeaths(string charID, DateTime year) {
            string db = $"wrapped_{year:yyyy}";

            using NpgsqlConnection conn = _DbHelper.Connection(db);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    from wt_deaths_{year:yyyy}
                    WHERE killed_character_id = @CharID
                        AND revived_event_id IS NULL;
            ");

            cmd.AddParameter("CharID", charID);

            List<KillEvent> evs = await _KillEventReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

    }

}
