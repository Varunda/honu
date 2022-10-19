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

        /// <summary>
        ///     Get a list of <see cref="WeaponStatTop"/> for an item
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        /// <returns>
        ///     A list of <see cref="WeaponStatTop"/> with <see cref="WeaponStatTop.ItemID"/> of <paramref name="itemID"/>.
        ///     If an empty list is returned, it means the stats haven't been generated, or don't exist
        /// </returns>
        public async Task<List<WeaponStatTop>> GetByItemID(int itemID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM weapon_stat_top_xref
                    WHERE item_id = @ItemID;
            ");

            cmd.AddParameter("ItemID", itemID);

            List<WeaponStatTop> top = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return top;
        }

        /// <summary>
        ///     Set the <see cref="WeaponStatTop"/>s of an item
        /// </summary>
        /// <remarks>
        ///     This is done with a transaction, that first deletes all entries with <see cref="WeaponStatTop.ItemID"/>
        ///     of <paramref name="itemID"/>, then inserts the data passed in <paramref name="tops"/>
        /// </remarks>
        /// <param name="itemID">ID of the item</param>
        /// <param name="tops">List of top weapon stats</param>
        public async Task SetByItemID(int itemID, List<WeaponStatTop> tops) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            await conn.OpenAsync();
            using NpgsqlTransaction trans = await conn.BeginTransactionAsync();

            try {
                using NpgsqlCommand deleteCmd = conn.CreateCommand();
                deleteCmd.CommandType = CommandType.Text;
                deleteCmd.CommandText = @"
                    DELETE FROM weapon_stat_top_xref
                        WHERE item_id = @ItemID;
                ";

                deleteCmd.AddParameter("ItemID", itemID);

                await deleteCmd.ExecuteNonQueryAsync();

                using NpgsqlCommand insertCmd = conn.CreateCommand();
                insertCmd.CommandType = CommandType.Text;
                insertCmd.CommandText = @"
                    INSERT INTO weapon_stat_top_xref (
                        world_id, faction_id, type_id, timestamp,
                        item_id, character_id, vehicle_id,
                        kills, deaths,
                        shots, shots_hit, headshots, vehicle_kills, seconds_with,
                        kd, kpm, acc, hsr, vkpm
                    ) VALUES (
                        @WorldID, @FactionID, @TypeID, @Timestamp,
                        @ItemID, @CharacterID, @VehicleID,
                        @Kills, @Deaths,
                        @Shots, @ShotsHit, @Headshots, @VehicleKills, @SecondsWith,
                        @KD, @KPM, @Acc, @Hsr, @VKPM
                    );
                ";

                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "WorldID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "FactionID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "TypeID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Timestamp" } );

                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "ItemID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "CharacterID" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "VehicleID" } );

                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Kills" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Deaths" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Shots" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "ShotsHit" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Headshots" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "VehicleKills" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "SecondsWith" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "KD" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "KPM" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Acc" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "Hsr" } );
                insertCmd.Parameters.Add(new NpgsqlParameter() { ParameterName = "VKPM" } );

                foreach (WeaponStatTop top in tops) {
                    decimal kd = top.Kills / Math.Max(1m, top.Deaths);
                    decimal kpm = top.Kills / (Math.Max(1m, top.SecondsWith) / 60m);
                    decimal acc = top.ShotsHit / Math.Max(1m, top.Shots);
                    decimal hsr = top.Headshots / Math.Max(1m, top.Kills);
                    decimal vkpm = top.VehicleKills / (Math.Max(1m, top.SecondsWith) / 60m);

                    insertCmd.Parameters["WorldID"].Value = top.WorldID;
                    insertCmd.Parameters["FactionID"].Value = top.FactionID;
                    insertCmd.Parameters["TypeID"].Value = top.TypeID;
                    insertCmd.Parameters["Timestamp"].Value = top.Timestamp;

                    insertCmd.Parameters["ItemID"].Value = itemID;
                    insertCmd.Parameters["CharacterID"].Value = top.CharacterID;
                    insertCmd.Parameters["VehicleID"].Value = top.VehicleID;

                    insertCmd.Parameters["Kills"].Value = top.Kills;
                    insertCmd.Parameters["Deaths"].Value = top.Deaths;
                    insertCmd.Parameters["Shots"].Value = top.Shots;
                    insertCmd.Parameters["ShotsHit"].Value = top.ShotsHit;
                    insertCmd.Parameters["Headshots"].Value = top.Headshots;
                    insertCmd.Parameters["VehicleKills"].Value = top.VehicleKills;
                    insertCmd.Parameters["SecondsWith"].Value = top.SecondsWith;

                    insertCmd.Parameters["KD"].Value = kd;
                    insertCmd.Parameters["KPM"].Value = kpm;
                    insertCmd.Parameters["Acc"].Value = acc;
                    insertCmd.Parameters["Hsr"].Value = hsr;
                    insertCmd.Parameters["VKPM"].Value = vkpm;

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
