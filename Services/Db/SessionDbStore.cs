using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Queues;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the wt_session database table
    /// </summary>
    public class SessionDbStore : IDataReader<Session> {

        private readonly ILogger<SessionDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public SessionDbStore(ILogger<SessionDbStore> logger,
                IDbHelper helper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public async Task<List<Session>> GetUnfixed(CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE needs_fix = true
                    ORDER BY start DESC
                    LIMIT 100;
            ");

            List<Session> sessions = await ReadList(cmd, cancel);
            await conn.CloseAsync();

            return sessions;
        }

        public async Task<long> GetUnfixedCount() {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT COUNT(*)
                    FROM wt_session
                    WHERE needs_fix = true;
            ");

            long count = await cmd.ExecuteInt64(CancellationToken.None);
            await conn.CloseAsync();

            return count;
        }

        public async Task SetFixed(long sessionID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET needs_fix = false
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", sessionID);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get all sessions of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="Session"/> with <see cref="Session.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<Session>> GetAllByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<Session> sessions = await ReadList(cmd);
            await conn.CloseAsync();

            return sessions;
        }

        /// <summary>
        ///     Get a specific session
        /// </summary>
        /// <param name="sessionID">ID of the session to get</param>
        /// <returns>
        ///     The <see cref="Session"/> with <see cref="Session.ID"/> of <paramref name="sessionID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<Session?> GetByID(long sessionID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", sessionID);

            Session? session = await ReadSingle(cmd);
            await conn.CloseAsync();

            return session;
        }
        
        /// <summary>
        ///     Get all sessions of a charater between a time period
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<Session>> GetByRangeAndCharacterID(string charID, DateTime start, DateTime end) {
            if (start > end) {
                _Logger.LogWarning($"Warning, start comes after end, {start} > {end}");
            }

            using Activity? trace = HonuActivitySource.Root.StartActivity("session get by character id and range");
            trace?.AddTag("honu.characterID", charID);
            trace?.AddTag("honu.start", $"{start:u}");
            trace?.AddTag("honu.end", $"{end:u}");

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE character_id = @CharID
                        AND ((start BETWEEN @PeriodStart AND @PeriodEnd)
                            OR (finish BETWEEN @PeriodStart AND @PeriodEnd)
                            OR (start <= @PeriodStart AND finish >= @PeriodEnd)
                            OR (start >= @PeriodStart AND finish <= @PeriodEnd)
                            OR (start < @PeriodStart AND finish IS NULL)
                        )
            ");

            cmd.AddParameter("CharID", charID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            //_Logger.LogTrace($"{cmd.Print()}");

            List<Session> sessions = await ReadList(cmd);
            await conn.CloseAsync();

            return sessions;
        }

        /// <summary>
        ///     Get all sessions that occured between a time period
        ///     (all sessions that started, ended, or were over this range)
        /// </summary>
        /// <param name="start">start of the range (inclusive)</param>
        /// <param name="end">end of the range (exclusive)</param>
        /// <returns>
        ///     all <see cref="Session"/>s with a <see cref="Session.Start"/> between <paramref name="start"/> and <paramref name="end"/>,
        ///     a <see cref="Session.End"/> between <paramref name="start"/> and <paramref name="end"/>,
        ///     or where <see cref="Session.Start"/> is greater than <paramref name="start"/>
        ///     AND <see cref="Session.End"/> is greater than <paramref name="end"/>
        /// </returns>
        public async Task<List<Session>> GetByRange(DateTime start, DateTime end) {
            if (start > end) {
                throw new ArgumentException($"{nameof(start)} cannot come after {nameof(end)}");
            }

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE (
                        (start BETWEEN @PeriodStart AND @PeriodEnd)
                        OR (finish BETWEEN @PeriodStart AND @PeriodEnd)
                        OR (start <= @PeriodStart AND finish >= @PeriodEnd)
                        OR (start < @PeriodStart AND finish IS NULL)
                    );
            ");

            cmd.CommandTimeout = 300;

            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            List<Session> sessions = await ReadList(cmd);
            await conn.CloseAsync();

            return sessions;
        }

        /// <summary>
        ///     Get all sessions between two ranges within an outfit
        /// </summary>
        /// <param name="outfitID">Optional ID of the outfit to provide</param>
        /// <param name="start">Start range</param>
        /// <param name="end">End range</param>
        /// <returns>
        ///     A list of all <see cref="Session"/>s that occured between <paramref name="start"/> and <paramref name="end"/>,
        ///     with <see cref="Session.OutfitID"/> of <paramref name="outfitID"/>
        /// </returns>
        public async Task<List<Session>> GetByRangeAndOutfit(string? outfitID, DateTime start, DateTime end) {
            if (start > end) {
                _Logger.LogWarning($"Warning, start comes after end, {start} > {end}");
            }

            using Activity? trace = HonuActivitySource.Root.StartActivity("session get by outfit id and range");
            trace?.AddTag("honu.outfitID", outfitID);
            trace?.AddTag("honu.start", $"{start:u}");
            trace?.AddTag("honu.end", $"{end:u}");

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE outfit_id = @OutfitID
                        AND ((start BETWEEN @PeriodStart AND @PeriodEnd)
                            OR (finish BETWEEN @PeriodStart AND @PeriodEnd)
                            OR (start <= @PeriodStart AND finish >= @PeriodEnd)
                            OR (start >= @PeriodStart AND finish <= @PeriodEnd)
                            OR (start < @PeriodStart AND finish IS NULL)
                        )
            ");

            cmd.AddParameter("OutfitID", outfitID);
            cmd.AddParameter("PeriodStart", start);
            cmd.AddParameter("PeriodEnd", end);

            //_Logger.LogTrace($"{cmd.Print()}");

            List<Session> sessions = await ReadList(cmd);
            await conn.CloseAsync();

            return sessions;
        }

        /// <summary>
        ///     Insert a new <see cref="Session"/>
        /// </summary>
        /// <param name="session">ID of the session</param>
        /// <returns>
        ///     The <see cref="Session.ID"/> of the row that was just inserted, or <c>null</c>
        /// </returns>
        public async Task<long> Insert(Session session) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = (@Timestamp - '1 second'::INTERVAL)
                    WHERE character_id = @CharacterID 
                        AND finish IS NULL;

                INSERT INTO wt_session (
                    character_id, start, finish, outfit_id, team_id
                ) VALUES (
                    @CharacterID, @Timestamp, null, @OutfitID, @TeamID
                ) RETURNING id;
            ");

            cmd.AddParameter("CharacterID", session.CharacterID);
            cmd.AddParameter("Timestamp", session.Start);
            cmd.AddParameter("OutfitID", session.OutfitID);
            cmd.AddParameter("TeamID", session.TeamID);
            await cmd.PrepareAsync();

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        /// <summary>
        ///     Set the <see cref="Session.End"/> of a session
        /// </summary>
        /// <param name="sessionID">ID of the session to update</param>
        /// <param name="when">What value of <see cref="Session.End"/> will have</param>
        public async Task SetSessionEndByID(long sessionID, DateTime when) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = @Timestamp
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", sessionID);
            cmd.AddParameter("Timestamp", when);
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     End all sessions currently opened in the DB
        /// </summary>
        /// <param name="when">What the finish value will be set to</param>
        /// <param name="cancel">Cancelation token</param>
        public async Task EndAll(DateTime when, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = @Timestamp
                    WHERE finish IS NULL; 
            ");

            cmd.AddParameter("Timestamp", when);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get the first session that occured, or null if no sessions have occured
        /// </summary>
        /// <remarks>
        ///     This is used for creating population data
        /// </remarks>
        public async Task<Session?> GetFirstSession() {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    ORDER BY id ASC
                    LIMIT 1;
            ");

            Session? f = await ReadSingle(cmd);
            await conn.CloseAsync();

            return f;
        }

        public async Task UpdateSummary(long sessionID, Session s) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE 
                    wt_session
                SET 
                    summary_calculated = @SummaryCalculated,
                    kills = @Kills,
                    deaths = @Deaths,
                    vehicle_kills = @VehicleKills,
                    heals = @Heals,
                    revives = @Revives,
                    shield_repairs = @ShieldRepairs,
                    resupplies = @Resupplies,
                    repairs = @Repairs,
                    spawns = @Spawns,
                    experience_gained = @ExperienceGained
                WHERE id = @SessionID;
            ");

            cmd.AddParameter("SessionID", sessionID);
            cmd.AddParameter("SummaryCalculated", s.SummaryCalculated);
            cmd.AddParameter("Kills", s.Kills);
            cmd.AddParameter("Deaths", s.Deaths);
            cmd.AddParameter("VehicleKills", s.VehicleKills);
            cmd.AddParameter("ExperienceGained", s.ExperienceGained);
            cmd.AddParameter("Heals", s.Heals);
            cmd.AddParameter("Revives", s.Revives);
            cmd.AddParameter("ShieldRepairs", s.ShieldRepairs);
            cmd.AddParameter("Resupplies", s.Resupplies);
            cmd.AddParameter("Repairs", s.Repairs);
            cmd.AddParameter("Spawns", s.Spawns);
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override Session ReadEntry(NpgsqlDataReader reader) {
            Session s = new();

            s.ID = reader.GetInt64("id");
            s.CharacterID = reader.GetString("character_id");
            s.Start = reader.GetDateTime("start");
            s.End = reader.GetNullableDateTime("finish");
            s.OutfitID = reader.GetNullableString("outfit_id");
            s.TeamID = reader.GetInt16("team_id");

            s.SummaryCalculated = reader.GetNullableDateTime("summary_calculated");
            s.Kills = reader.GetInt32("kills");
            s.Deaths = reader.GetInt32("deaths");
            s.VehicleKills = reader.GetInt32("vehicle_kills");
            s.ExperienceGained = reader.GetInt64("experience_gained");
            s.Heals = reader.GetInt32("heals");
            s.Revives = reader.GetInt32("revives");
            s.ShieldRepairs = reader.GetInt32("shield_repairs");
            s.Resupplies = reader.GetInt32("resupplies");
            s.Repairs = reader.GetInt32("repairs");
            s.Spawns = reader.GetInt32("spawns");

            return s;
        }
    }

}