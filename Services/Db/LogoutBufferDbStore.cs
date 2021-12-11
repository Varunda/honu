using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Queues;

namespace watchtower.Services.Db {

    /// <summary>
    ///     DB store for managing the <see cref="LogoutBufferEntry"/>s in the logout buffer
    /// </summary>
    public class LogoutBufferDbStore {

        private readonly ILogger<LogoutBufferDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<LogoutBufferEntry> _Reader;

        public LogoutBufferDbStore(ILogger<LogoutBufferDbStore> logger,
            IDbHelper helper, IDataReader<LogoutBufferEntry> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get all <see cref="LogoutBufferEntry"/>s currently waiting to be processed
        /// </summary>
        /// <param name="cancel">Cancel token</param>
        public async Task<List<LogoutBufferEntry>> GetPending(CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM logout_buffer;
            ");

            List<LogoutBufferEntry> entries = await _Reader.ReadList(cmd, cancel);
            await conn.CloseAsync();

            return entries;
        }

        /// <summary>
        ///     Delete the <see cref="LogoutBufferEntry"/> a character has
        /// </summary>
        /// <param name="charID">ID of the character to remove</param>
        /// <param name="cancel">Cancel token</param>
        public async Task Delete(string charID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE FROM logout_buffer
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Update/Insert a <see cref="LogoutBufferEntry"/>. Updates are based on character id
        /// </summary>
        /// <param name="entry"><see cref="LogoutBufferEntry"/> to update/insert</param>
        /// <param name="cancel">Cancel token</param>
        public async Task Upsert(LogoutBufferEntry entry, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO logout_buffer (
                    character_id, login_time, timestamp, not_found_count
                ) VALUES (
                    @CharacterID, @LoginTime, @Timestamp, 0
                ) ON CONFLICT (character_id) DO UPDATE
                    SET not_found_count = @NotFoundCount,
                        login_time = @LoginTime
            ");

            cmd.AddParameter("CharacterID", entry.CharacterID);
            cmd.AddParameter("LoginTime", entry.LoginTime);
            cmd.AddParameter("Timestamp", entry.Timestamp);
            cmd.AddParameter("NotFoundCount", entry.NotFoundCount);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

    }
}
