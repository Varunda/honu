
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;

namespace watchtower.Services.Db {

    /// <summary>
    ///     DB service that interacts with the metadata table
    /// </summary>
    public class HonuMetadataDbStore {

        private readonly ILogger<HonuMetadataDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public HonuMetadataDbStore(ILogger<HonuMetadataDbStore> logger, IDbHelper dbHelper) {
            _Logger = logger;
            _DbHelper = dbHelper;
        }

        /// <summary>
        ///     Read a single value from the metadata DB
        /// </summary>
        /// <param name="key">key of the object to get</param>
        /// <returns>
        ///     The string value of the metadata key, or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<string?> Get(string key) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT value
                    from metadata
                    WHERE name = @Key;
            ");

            cmd.AddParameter("Key", key);

            object? obj = await cmd.ExecuteScalarAsync();
            if (obj == null) {
                return null;
            }

            if (obj is string) {
                return obj as string;
            }

            _Logger.LogError($"failed to read metadata key, column was not a string! [Key={key}]");

            return null;
        }

        /// <summary>
        ///     Update/Insert (upsert) a value in the metadata DB table
        /// </summary>
        /// <param name="key">key to upsert</param>
        /// <param name="value">value to set</param>
        /// <returns>
        ///     A task for when the async operation is complete
        /// </returns>
        public async Task Upsert(string key, string value) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO metadata (name, value)
                    VALUES (@Key, @Value)
                ON CONFLICT (name) DO
                    UPDATE SET value = @Value;
            ");

            cmd.AddParameter("Key", key);
            cmd.AddParameter("Value", value);

            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

    }

}
