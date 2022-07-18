using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class ExpEventDbStore : IDataReader<ExpDbEntry> {

        private readonly ILogger<ExpEventDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<ExpEvent> _ExpDataReader;

        private const string unformatted = @"
            INSERT INTO {0} (
                source_character_id, experience_id, source_loadout_id,
                source_faction_id, source_team_id,
                other_id,
                amount,
                world_id, zone_id,
                timestamp
            ) VALUES (
                $1, $2, $3,
                $4, $5,
                $6,
                $7,
                $8, $9,
                $10
            )
        ";

        public ExpEventDbStore(ILogger<ExpEventDbStore> logger,
            IDbHelper dbHelper, IDataReader<ExpEvent> expReader) {

            _Logger = logger;
            _DbHelper = dbHelper;

            _ExpDataReader = expReader ?? throw new ArgumentNullException(nameof(expReader));
        }

        /// <summary>
        ///     Insert a new <see cref="ExpEvent"/>, returning the ID of the row created
        /// </summary>
        /// <param name="ev">Parameters used to insert the event</param>
        /// <returns>
        ///     The ID of the event that was just inserted into the table
        /// </returns>
        public async Task<long> Insert(ExpEvent ev) {
            await using NpgsqlConnection conn = _DbHelper.Connection(task: "exp insert", enlist: false);
            await conn.OpenAsync();

            // Is there a way to save the Parameters and share it between the commands? 
            await using NpgsqlBatch batch = new NpgsqlBatch(conn) {
                BatchCommands = {
                    new NpgsqlBatchCommand() {
                        CommandText = string.Format(unformatted, "wt_recent_exp") + ";",
                        Parameters = {
                            new() { Value = ev.SourceID }, new() { Value = ev.ExperienceID }, new() { Value = ev.LoadoutID },
                            new() { Value = Loadout.GetFaction(ev.LoadoutID) }, new() { Value = ev.TeamID },
                            new() { Value = ev.OtherID },
                            new() { Value = ev.Amount },
                            new() { Value = ev.WorldID }, new() { Value = unchecked((int)ev.ZoneID) },
                            new() { Value = ev.Timestamp }
                        }
                    },

                    new NpgsqlBatchCommand() {
                        CommandText = string.Format(unformatted, "wt_exp") + " RETURNING id;",
                        Parameters = {
                            new() { Value = ev.SourceID }, new() { Value = ev.ExperienceID }, new() { Value = ev.LoadoutID },
                            new() { Value = Loadout.GetFaction(ev.LoadoutID) }, new() { Value = ev.TeamID },
                            new() { Value = ev.OtherID },
                            new() { Value = ev.Amount },
                            new() { Value = ev.WorldID }, new() { Value = unchecked((int)ev.ZoneID) },
                            new() { Value = ev.Timestamp }
                        }
                    }

                }
            };

            await batch.PrepareAsync();

            object? IDobj = await batch.ExecuteScalarAsync();
            await conn.CloseAsync();
            if (IDobj == null) {
                throw new NullReferenceException($"The scalar returned when inserting a kill was null");
            }

            return (long)IDobj;

            /*
            await using NpgsqlCommand cmd = new NpgsqlCommand(@"
                INSERT INTO wt_exp (
                    source_character_id, experience_id, source_loadout_id,
                    source_faction_id, source_team_id,
                    other_id,
                    amount,
                    world_id, zone_id,
                    timestamp
                ) VALUES (
                    $1, $2, $3,
                    $4, $5,
                    $6,
                    $7,
                    $8, $9,
                    $10
                ) RETURNING id;
            ", conn) {
                Parameters = {
                    new() { Value = ev.SourceID }, new() { Value = ev.ExperienceID }, new() { Value = ev.LoadoutID },
                    new() { Value = Loadout.GetFaction(ev.LoadoutID) }, new() { Value = ev.TeamID },
                    new() { Value = ev.OtherID },
                    new() { Value = ev.Amount },
                    new() { Value = ev.WorldID }, new() { Value = unchecked((int)ev.ZoneID) },
                    new() { Value = ev.Timestamp }
                }
            };

            await cmd.PrepareAsync();
            */

            /*
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_exp (
                    source_character_id, experience_id, source_loadout_id, source_faction_id, source_team_id,
                    other_id,
                    amount,
                    world_id, zone_id,
                    timestamp
                ) VALUES (
                    @SourceCharacterID, @ExperienceID, @SourceLoadoutID, @SourceFactionID, @SourceTeamID,
                    @OtherID,
                    @Amount,
                    @WorldID, @ZoneID,
                    @Timestamp
                ) RETURNING id;
            ");
            */

            //long ID = await cmd.ExecuteInt64(CancellationToken.None);
            //return ID;
        }

        /// <summary>
        ///     Get the top players who have performed an action specified in <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters">Parameters used to performed the action</param>
        /// <returns>
        ///     The top players who have met the parameters passed in <paramref name="parameters"/>
        /// </returns>
        public async Task<List<ExpDbEntry>> GetEntries(ExpEntryOptions parameters) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT source_character_id AS id, COUNT(source_character_id) AS count
	                FROM wt_recent_exp
                    WHERE timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                        AND world_id = @WorldID
		                AND experience_id = ANY(@ExperienceIDs)
                        AND source_team_id = @FactionID
	                GROUP BY source_character_id
	                ORDER BY COUNT(source_character_id) DESC
	                LIMIT 5;
            ");

            cmd.AddParameter("Interval", parameters.Interval);
            cmd.AddParameter("WorldID", parameters.WorldID);
            cmd.AddParameter("ExperienceIDs", parameters.ExperienceIDs);
            cmd.AddParameter("FactionID", parameters.FactionID);
            await cmd.PrepareAsync();

            List<ExpDbEntry> entries = await ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Get the outfits who have performed an action specified in <paramref name="options"/>
        /// </summary>
        /// <param name="options">Options to filter the entries returned</param>
        /// <returns>
        ///     A list of outfits who have met the parameters passed in <paramref name="options"/>
        /// </returns>
        public async Task<List<ExpDbEntry>> GetTopOutfits(ExpEntryOptions options) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH evs AS (
                    SELECT e.id, e.world_id, e.zone_id, COALESCE(c.outfit_id, '') AS outfit_id
                        FROM wt_recent_exp e
                            JOIN wt_character c ON e.source_character_id = c.id
                        WHERE e.timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                            AND e.world_id = @WorldID
                            AND e.source_team_id = @FactionID
                            AND e.experience_id = ANY(@ExperienceIDs)
                ), outfits AS (
                    SELECT outfit_id
                        FROM evs
                        GROUP BY outfit_id
                )
                SELECT outfits.outfit_id AS id,
                    (SELECT COUNT(*) FROM evs e WHERE e.outfit_id = outfits.outfit_id) AS count
                    FROM outfits
                    ORDER BY count DESC
            ");

            cmd.AddParameter("Interval", options.Interval);
            cmd.AddParameter("WorldID", options.WorldID);
            cmd.AddParameter("ExperienceIDs", options.ExperienceIDs);
            cmd.AddParameter("FactionID", options.FactionID);
            await cmd.PrepareAsync();

            List<ExpDbEntry> entries = await ReadList(cmd);
            await conn.CloseAsync();

            return entries;
        }

        public async Task<List<ExpEvent>> GetByCharacterID(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_exp
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND source_character_id = @CharacterID
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);
            await cmd.PrepareAsync();

            List<ExpEvent> events = await _ExpDataReader.ReadList(cmd);
            await conn.CloseAsync();

            return events;
        }

        public async Task<List<ExpEvent>> GetByCharacterIDs(List<string> IDs, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_exp
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                        AND source_character_id = ANY(@IDs)
            ");

            cmd.AddParameter("IDs", IDs);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<ExpEvent> events = await _ExpDataReader.ReadList(cmd);
            await conn.CloseAsync();

            return events;
        }

        public async Task<List<ExpEvent>> GetByOutfitID(string outfitID, short worldID, short teamID, int interval) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT wt_exp.*
                    FROM wt_exp
                        INNER JOIN wt_character ON wt_exp.source_character_id = wt_character.id
                    WHERE wt_exp.timestamp >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL)
                        AND wt_exp.world_id = @WorldID
                        AND wt_exp.source_team_id = @TeamID
                        AND (wt_character.outfit_id = @OutfitID OR (@OutfitID = '0' AND wt_character.outfit_id IS NULL));
            ");

            cmd.AddParameter("OutfitID", outfitID);
            cmd.AddParameter("WorldID", worldID);
            cmd.AddParameter("TeamID", teamID);
            cmd.AddParameter("Interval", interval);

            List<ExpEvent> events = await _ExpDataReader.ReadList(cmd);
            await conn.CloseAsync();

            return events;
        }

        /// <summary>
        ///     Get the exp events that occured between a time period, optionally limiting to a zone and world
        /// </summary>
        /// <param name="start">Start period</param>
        /// <param name="end">End period</param>
        /// <param name="zoneID">Optional, zone ID to limit the kills by</param>
        /// <param name="worldID">Optional, world ID to limit the kills by</param>
        /// <returns>
        ///     All <see cref="ExpEvent"/>s that occured between the range given. If <paramref name="zoneID"/>
        ///     and/or <paramref name="worldID"/> is given, the event will match those options given
        /// </returns>
        public async Task<List<ExpEvent>> GetByRange(DateTime start, DateTime end, uint? zoneID, short? worldID) {
            bool useAll = (DateTime.UtcNow - end) > TimeSpan.FromHours(2) || (DateTime.UtcNow - start) > TimeSpan.FromHours(2);

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT *
                    FROM {(useAll ? "wt_exp" : "wt_recent_exp")}
                    WHERE timestamp BETWEEN @PeriodStart AND @PeriodEnd
                    {(zoneID != null ? " AND zone_id = @ZoneID " : "")}
                    {(worldID != null ? " AND world_id = @WorldID " : "")}
            ");

            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);
            cmd.AddParameter("ZoneID", zoneID);
            cmd.AddParameter("WorldID", worldID);
            await cmd.PrepareAsync();

            List<ExpEvent> evs = await _ExpDataReader.ReadList(cmd);
            await conn.CloseAsync();

            return evs;
        }

        public override ExpDbEntry ReadEntry(NpgsqlDataReader reader) {
            ExpDbEntry entry = new ExpDbEntry();

            entry.ID = reader.GetString("id");
            entry.Count = reader.GetInt32("count");

            return entry;
        }

    }

    public static class IExpEventDbStoreExtensionMethods {

        public static Task<List<ExpEvent>> GetRecentByCharacterID(this ExpEventDbStore db, string characterID, int interval) {
            DateTime start = DateTime.UtcNow - TimeSpan.FromSeconds(interval * 60);
            return db.GetByCharacterID(characterID, start, DateTime.UtcNow);
        }

    }

}
