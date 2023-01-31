using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class SessionEndSubscriptionDbStore {

        private readonly ILogger<SessionEndSubscriptionDbStore> _Logger;
        private readonly IDataReader<SessionEndSubscription> _Reader;
        private readonly IDbHelper _DbHelper;

        public SessionEndSubscriptionDbStore(ILogger<SessionEndSubscriptionDbStore> logger,
            IDataReader<SessionEndSubscription> reader, IDbHelper dbHelper) {

            _Logger = logger;
            _Reader = reader;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Insert a new <see cref="SessionEndSubscription"/>
        /// </summary>
        /// <param name="sub">Subscription to be added</param>
        /// <returns>The <see cref="SessionEndSubscription.ID"/> of the newly created entry</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<long> Insert(SessionEndSubscription sub) {
            if (sub.DiscordID == 0) {
                throw new ArgumentException($"{nameof(SessionEndSubscription.DiscordID)} cannot be 0");
            }
            if (string.IsNullOrEmpty(sub.CharacterID)) {
                throw new ArgumentException($"{nameof(SessionEndSubscription.CharacterID)} cannot be empty");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO session_end_subscription (
                    discord_id, character_id, timestamp
                ) VALUES (
                    @DiscordID, @CharacterID, NOW() AT TIME ZONE 'utc'
                ) RETURNING id;
            ");

            cmd.AddParameter("DiscordID", sub.DiscordID);
            cmd.AddParameter("CharacterID", sub.CharacterID);

            long id = await cmd.ExecuteInt64(CancellationToken.None);

            return id;
        }

        public async Task<SessionEndSubscription?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM session_end_subscription
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            SessionEndSubscription? sub = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return sub;
        }

        public async Task<List<SessionEndSubscription>> GetByDiscordID(ulong discordID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM session_end_subscription
                    WHERE discord_id = @DiscordID;
            ");

            cmd.AddParameter("DiscordID", discordID);

            List<SessionEndSubscription> sub = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return sub;
        }

        public async Task<List<SessionEndSubscription>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM session_end_subscription
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<SessionEndSubscription> sub = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return sub;
        }

        /// <summary>
        ///     Delete a specific subscription by name
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task DeleteByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE FROM session_end_subscription
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
