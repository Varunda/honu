using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db {

    public class WeaponStatSnapshotDbStore {

        private readonly ILogger<WeaponStatSnapshotDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WeaponStatSnapshot> _Reader;

        public WeaponStatSnapshotDbStore(ILogger<WeaponStatSnapshotDbStore> logger,
            IDbHelper dbHelper, IDataReader<WeaponStatSnapshot> reader) {

            _Logger = logger;

            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a <see cref="WeaponStatSnapshot"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the entry to get</param>
        /// <returns></returns>
        public async Task<WeaponStatSnapshot?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_snapshot
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            WeaponStatSnapshot? snapshot = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return snapshot;
        }

        public async Task<List<WeaponStatSnapshot>> GetByItemID(int itemID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_snapshot
                    WHERE item_id = @ItemID;
            ");

            cmd.AddParameter("item_id", itemID);

            List<WeaponStatSnapshot> snapshots = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return snapshots;
        }

        public async Task Generate(CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO weapon_stat_snapshot (
                    item_id, timestamp, users, kills, deaths, headshots, shots, shots_hit, vehicle_kills, seconds_with
                ) SELECT CAST(item_id AS int), NOW() AT TIME ZONE 'utc', COUNT(DISTINCT(character_id)), SUM(kills), sum(deaths), sum(headshots), sum(shots), sum(shots_hit), sum(vehicle_kills), sum(seconds_with)
                    FROM weapon_stats
                    WHERE kills > 1159
                    GROUP BY item_id
                    HAVING sum(kills) > 0;
            ");

            cmd.CommandTimeout = 60 * 60; // 60 minutes

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Get the most recent <see cref="WeaponStatSnapshot"/>
        /// </summary>
        /// <returns></returns>
        public async Task<DateTime?> GetMostRecent() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT MAX(timestamp)
                    FROM weapon_stat_snapshot;
            ");

            object? obj = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();

            if (obj != null && obj is not DBNull) {
                return (DateTime)obj;
            }

            return null;
        }

    }
}
