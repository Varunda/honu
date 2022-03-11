using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;

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
            using NpgsqlConnection conn = _DbHelper.Connection();
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
            using NpgsqlConnection conn = _DbHelper.Connection();
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
            using NpgsqlConnection conn = _DbHelper.Connection();
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
        ///     Get the sessions of a character that are within the interval from now
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="interval">How many minutes back to go</param>
        /// <returns>
        ///     All <see cref="Session"/>s within the period given
        /// </returns>
        public async Task<List<Session>> GetByCharacterID(string charID, int interval) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE character_id = @CharacterID
                        AND (finish IS NULL OR finish >= (NOW() at time zone 'utc' - (@Interval || ' minutes')::INTERVAL))
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("Interval", interval);

            List<Session> sessions = await ReadList(cmd);
            await conn.CloseAsync();

            return sessions;
        }

        /// <summary>
        ///     Get all sessions of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="Session"/> with <see cref="Session.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<Session>> GetAllByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
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
            using NpgsqlConnection conn = _DbHelper.Connection();
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

            using NpgsqlConnection conn = _DbHelper.Connection();
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
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<Session>> GetByRange(DateTime start, DateTime end) {
            if (start > end) {
                throw new ArgumentException($"{nameof(start)} cannot come after {nameof(end)}");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_session
                    WHERE (
                        (start BETWEEN @PeriodStart AND @PeriodEnd)
                        OR (finish BETWEEN @PeriodStart AND @PeriodEnd)
                        OR (start <= @PeriodStart AND finish >= @PeriodEnd)
                        OR (start >= @PeriodStart AND finish <= @PeriodEnd)
                        OR (start < @PeriodStart AND finish IS NULL)
                    );
            ");

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

            using NpgsqlConnection conn = _DbHelper.Connection();
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
        ///     Start a new session of a tracked player
        /// </summary>
        /// <param name="player">Player that will have a new session</param>
        /// <param name="when">When the session started</param>
        /// <returns>
        ///     A task for when the task is completed
        /// </returns>
        public async Task Start(string charID, DateTime when) {

            TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(charID);
            if (player == null) {
                _Logger.LogError($"Cannot start session for {charID}, does not exist in CharacterStore");
                return;
            }

            if (player.Online == true) {
                //_Logger.LogWarning($"Not starting session for {player.ID}, logged in at {player.LastLogin}");
                return;
            }

            // Insert the outfit_id and team_id based on what's in wt_character, and the TrackedPlayer might not have that data set yet
            NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = NOW() at time zone 'utc'
                    WHERE character_id = @CharacterID 
                        AND finish IS NULL;
                
                INSERT INTO wt_session (
                    character_id, start, finish, outfit_id, team_id
                )
                SELECT @CharacterID, @Timestamp, null, c.outfit_id, c.faction_id
                    FROM wt_character c
                    WHERE c.id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", player.ID);
            cmd.AddParameter("Timestamp", when);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            player.Online = true;
            CharacterStore.Get().SetByCharacterID(charID, player);
        }

        /// <summary>
        ///     End an existing session of a tracked player
        /// </summary>
        /// <param name="player">Player who's session is endign</param>
        /// <param name="when">When the session ended</param>
        /// <returns>
        ///     A task for when the task is complete
        /// </returns>
        public async Task End(string charID, DateTime when) {
            TrackedPlayer? player = CharacterStore.Get().GetByCharacterID(charID);
            if (player == null) {
                _Logger.LogError($"Cannot start session for {charID}, does not exist in CharacterStore");
                return;
            }

            if (player.Online == false) {
                //_Logger.LogWarning($"Player {player.ID} is already offline, might not have a session to end");
                return;
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = @Timestamp,
                        team_id = @TeamID,
                        outfit_id = @OutfitID
                    WHERE id = (
                        SELECT MAX(id)
                            FROM wt_session 
                            WHERE character_id = @CharacterID
                    ) AND finish IS NULL;
            ");

            // Until I know where the -1 values are coming from, set it to saner value
            short teamID = player.TeamID;
            if (teamID == -1 || teamID == 0) {
                teamID = player.FactionID;
            }

            cmd.AddParameter("CharacterID", player.ID);
            cmd.AddParameter("OutfitID", player.OutfitID);
            cmd.AddParameter("TeamID", teamID);
            cmd.AddParameter("Timestamp", when);

            player.Online = false;

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            CharacterStore.Get().SetByCharacterID(charID, player);
        }

        /// <summary>
        ///     End all sessions currently opened in the DB
        /// </summary>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        public async Task EndAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = NOW() at time zone 'utc'
                    WHERE finish IS NULL; 
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override Session ReadEntry(NpgsqlDataReader reader) {
            Session s = new Session();

            s.ID = reader.GetInt64("id");
            s.CharacterID = reader.GetString("character_id");
            s.Start = reader.GetDateTime("start");
            s.End = reader.GetNullableDateTime("finish");
            s.OutfitID = reader.GetNullableString("outfit_id");
            s.TeamID = reader.GetInt16("team_id");

            return s;
        }
    }

}