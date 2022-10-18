using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class WeaponStatTopDbStore {

        private readonly ILogger<WeaponStatTopDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WeaponStatTop> _Reader;

        public WeaponStatTopDbStore(ILogger<WeaponStatTopDbStore> logger,
            IDbHelper dbHelper, IDataReader<WeaponStatTop> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a <see cref="WeaponStatTop"/> by it's ID
        /// </summary>
        /// <param name="ID">ID of the entry to get</param>
        /// <returns></returns>
        public async Task<WeaponStatTop?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_top_xref
                    id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            WeaponStatTop? top = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return top;
        }

        public async Task SetByItemID(int itemID, List<WeaponStatTop> tops) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            await conn.OpenAsync();
            using NpgsqlTransaction trans = await conn.BeginTransactionAsync();

            try {
                using NpgsqlCommand deleteCmd = conn.CreateCommand();
                deleteCmd.CommandType = CommandType.Text;
                deleteCmd.CommandText = @"
                    DELETE FROM weapon_stat_top
                        WHERE item_id = @ItemID;
                ";

                deleteCmd.AddParameter("ItemID", itemID);

                await deleteCmd.ExecuteNonQueryAsync();

                using NpgsqlCommand insertCmd = conn.CreateCommand();
                insertCmd.CommandType = CommandType.Text;
                insertCmd.CommandText = @"
                    INSERT INTO weapon_stat_top (
                        item_id, world_id, faction_id, type_id, timestamp, reference_id
                    ) VALUES (
                        @ItemID, @WorldID, @FactionID, @TypeID, @Timestamp, @ReferenceID
                    );
                ";

                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "ItemID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "WorldID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "FactionID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "TypeID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Timestamp" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "ReferenceID" } );

                foreach (WeaponStatTop top in tops) {
                    insertCmd.Parameters["ItemID"].Value = itemID;
                    insertCmd.Parameters["TypeID"].Value = top.TypeID;
                    insertCmd.Parameters["Timestamp"].Value = top.Timestamp;
                    insertCmd.Parameters["WorldID"].Value = top.WorldID;
                    insertCmd.Parameters["FactionID"].Value = top.FactionID;
                    insertCmd.Parameters["ReferenceID"].Value = top.ReferenceID;
                    await insertCmd.ExecuteNonQueryAsync();
                }

                await trans.CommitAsync();
            } catch (Exception ex) {
                _Logger.LogError(ex, $"error in transaction when setting weapon stat top for {itemID}");
                await trans.RollbackAsync();
            } finally {
                await conn.CloseAsync();
            }

        }

    }
}
