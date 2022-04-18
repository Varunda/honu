using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db {

    public class WorldTagDbStore {

        private readonly ILogger<WorldTagDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WorldTagEntry> _Reader;

        public WorldTagDbStore(ILogger<WorldTagDbStore> logger,
            IDbHelper dbHelper, IDataReader<WorldTagEntry> reader) {
            
            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a <see cref="WorldTagEntry"/> by it's ID
        /// </summary>
        /// <param name="ID">ID of the <see cref="WorldTagEntry"/> to get</param>
        /// <returns>
        ///     The <see cref="WorldTagEntry"/> with <see cref="WorldTagEntry.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<WorldTagEntry?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM world_tag
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            WorldTagEntry? entry = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public async Task<long> Insert(WorldTagEntry entry) {
            if (entry.WasKilled == null) {
                throw new ArgumentNullException($"Cannot insert {nameof(WorldTagEntry)}, was_killed was null");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO world_tag (
                    character_id, world_id, timestamp, target_killed, kills, was_killed
                ) VALUES (
                    @CharacterID, @WorldID, @Timestamp, @TargetKilled, @Kills, @WasKilled
                ) RETURNING id;
            ");

            cmd.AddParameter("CharacterID", entry.CharacterID);
            cmd.AddParameter("WorldID", entry.WorldID);
            cmd.AddParameter("Timestamp", entry.Timestamp);
            cmd.AddParameter("TargetKilled", entry.TargetKilled);
            cmd.AddParameter("Kills", entry.Kills);
            cmd.AddParameter("WasKilled", entry.WasKilled);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

    }
}
