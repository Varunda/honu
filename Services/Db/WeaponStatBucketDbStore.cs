using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class WeaponStatBucketDbStore {

        private readonly ILogger<WeaponStatBucketDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WeaponStatBucket> _Reader;

        public WeaponStatBucketDbStore(ILogger<WeaponStatBucketDbStore> logger,
            IDbHelper dbHelper, IDataReader<WeaponStatBucket> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a single <see cref="WeaponStatBucket"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the <see cref="WeaponStatBucket"/> to get</param>
        /// <returns>
        ///     The <see cref="WeaponStatBucket"/> with <see cref="WeaponStatBucket.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public async Task<WeaponStatBucket?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_bucket
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            WeaponStatBucket? bucket = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return bucket;
        }

        /// <summary>
        ///     Get all <see cref="WeaponStatBucket"/> for an item
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <returns>
        ///     A list of <see cref="WeaponStatBucket"/> with <see cref="WeaponStatBucket.ItemID"/> of <paramref name="itemID"/>
        /// </returns>
        public async Task<List<WeaponStatBucket>> GetByItemID(int itemID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_bucket
                    WHERE item_id = @ItemID;
            ");

            cmd.AddParameter("ItemID", itemID);

            List<WeaponStatBucket> buckets = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return buckets;
        }

        public async Task SetByItemID(int itemID, short typeID, List<WeaponStatBucket> buckets) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            await conn.OpenAsync();
            using NpgsqlTransaction trans = await conn.BeginTransactionAsync();

            try {
                using NpgsqlCommand deleteCmd = conn.CreateCommand();
                deleteCmd.CommandType = CommandType.Text;
                deleteCmd.CommandText = @"
                    DELETE FROM weapon_stat_bucket
                        WHERE item_id = @ItemID
                            AND type_id = @TypeID;
                ";

                deleteCmd.AddParameter("ItemID", itemID);
                deleteCmd.AddParameter("TypeID", typeID);

                await deleteCmd.ExecuteNonQueryAsync();

                using NpgsqlCommand insertCmd = conn.CreateCommand();
                insertCmd.CommandType = CommandType.Text;
                insertCmd.CommandText = @"
                    INSERT INTO weapon_stat_bucket (
                        item_id, type_id, timestamp, start, width, count
                    ) VALUES (
                        @ItemID, @TypeID, @Timestamp, @Start, @Width, @Count
                    );
                ";

                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "ItemID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "TypeID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Timestamp" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Start" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Width" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Count" } );

                foreach (WeaponStatBucket bucket in buckets) {
                    insertCmd.Parameters["ItemID"].Value = itemID;
                    insertCmd.Parameters["TypeID"].Value = typeID;
                    insertCmd.Parameters["Timestamp"].Value = bucket.Timestamp;
                    insertCmd.Parameters["Start"].Value = bucket.Start;
                    insertCmd.Parameters["Width"].Value = bucket.Width;
                    insertCmd.Parameters["Count"].Value = bucket.Count;
                    await insertCmd.ExecuteNonQueryAsync();
                }

                await trans.CommitAsync();

            } catch (Exception ex) {
                _Logger.LogError(ex, $"error in transaction when setting weapon bucket stats for {itemID}");
                await trans.RollbackAsync();
            } finally {
                await conn.CloseAsync();
            }


        }

    }
}
