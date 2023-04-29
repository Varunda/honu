using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Wrapped;

namespace watchtower.Services.Db {

    public class WrappedDbStore {

        private readonly ILogger<WrappedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WrappedEntry> _Reader;

        public WrappedDbStore(ILogger<WrappedDbStore> logger,
            IDbHelper dbHelper, IDataReader<WrappedEntry> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a specific <see cref="WrappedEntry"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the <see cref="WrappedEntry"/> to get</param>
        /// <returns>
        ///     The <see cref="WrappedEntry"/> with <see cref="WrappedEntry.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<WrappedEntry?> GetByID(Guid ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wrapped_entries
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            WrappedEntry? entry = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return entry;
        }

        /// <summary>
        ///     Insert a new <see cref="WrappedEntry"/> to the DB
        /// </summary>
        /// <param name="entry">Entry to insert</param>
        /// <returns>
        ///     A task when this async operation is complete
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="entry"/> has a <see cref="WrappedEntry.ID"/> of <c>Guid.Empty</c>
        /// </exception>
        public async Task Insert(WrappedEntry entry) {
            if (entry.ID == Guid.Empty) {
                throw new ArgumentException($"not inserting {nameof(WrappedEntry)}: blank ID provided for characters: {string.Join(", ", entry.InputCharacterIDs)}");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wrapped_entries (
                    id, input_character_ids, timestamp, status
                ) VALUES (
                    @ID, @IDs, NOW() AT TIME ZONE 'utc', @Status
                );
            ");

            cmd.AddParameter("ID", entry.ID);
            cmd.AddParameter("IDs", string.Join(",", entry.InputCharacterIDs));
            cmd.AddParameter("Status", entry.Status);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Update the <see cref="WrappedEntry.Status"/> of a <see cref="WrappedEntry"/>
        /// </summary>
        /// <param name="ID">ID of the entry to update. Cannot be <see cref="Guid.Empty"/></param>
        /// <param name="status">New status to use</param>
        /// <exception cref="ArgumentException">If <paramref name="ID"/> is <see cref="Guid.Empty"/></exception>
        public async Task UpdateStatus(Guid ID, int status) {
            if (ID == Guid.Empty) {
                throw new ArgumentException($"not updated status of {nameof(WrappedEntry)}: provided ID is 0 ({ID})");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE wrapped_entries
                    SET status = @Status
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            cmd.AddParameter("Status", status);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
