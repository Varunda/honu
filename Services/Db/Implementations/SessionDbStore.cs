using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class SessionDbStore : IDataReader<Session>, ISessionDbStore {

        private readonly ILogger<SessionDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public SessionDbStore(ILogger<SessionDbStore> logger,
                IDbHelper helper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

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

        public async Task Start(TrackedPlayer player) {
            if (player.Online == true) {
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
                SELECT @CharacterID, NOW() at time zone 'utc', null, c.outfit_id, c.faction_id
                    FROM wt_character c
                    WHERE c.id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", player.ID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();

            player.Online = true;
        }

        public async Task End(TrackedPlayer player) {
            if (player.Online == false) {
                //_Logger.LogWarning($"Player {player.ID} is already offline, might not have a session to end");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wt_session
                    SET finish = NOW() at time zone 'utc',
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
            if (teamID == -1) {
                teamID = player.FactionID;
            }

            cmd.AddParameter("CharacterID", player.ID);
            cmd.AddParameter("OutfitID", player.OutfitID);
            cmd.AddParameter("TeamID", teamID);

            player.Online = false;

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

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
