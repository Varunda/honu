using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db {

    public class PsbNamedDbStore {

        private readonly ILogger<PsbNamedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsbNamedAccount> _Reader;

        public PsbNamedDbStore(ILogger<PsbNamedDbStore> logger,
            IDbHelper helper, IDataReader<PsbNamedAccount> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get all psb named account
        /// </summary>
        /// <returns></returns>
        public async Task<List<PsbNamedAccount>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named;
            ");

            List<PsbNamedAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        public async Task<List<PsbNamedAccount>> GetActive() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE deleted_at IS NOT NULL;
            ");

            List<PsbNamedAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        /// <summary>
        ///     Get a single named account by ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<PsbNamedAccount?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsbNamedAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        /// <summary>
        ///     Get the named account that has the tag and name (case sensitive)
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<PsbNamedAccount?> GetByTagAndName(string? tag, string name) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE tag = @Tag
                        AND name = @Name;
            ");

            cmd.AddParameter("Tag", tag);
            cmd.AddParameter("Name", name);

            PsbNamedAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        /// <summary>
        ///     Mark a named account as deleted. Does not actually delete from DB
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="deletedByID"></param>
        /// <returns></returns>
        public async Task Delete(long ID, long deletedByID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE psb_named
                    SET deleted_at = NOW() AT TIME ZONE 'utc',
                        deleted_by = @DeletedByID
                WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            cmd.AddParameter("DeletedByID", deletedByID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Insert a new <see cref="PsbNamedAccount"/>
        /// </summary>
        /// <param name="acc">Parameters used to insert</param>
        /// <returns>
        ///     The ID of the row just inserted
        /// </returns>
        /// <exception cref="Exception">
        ///     If the DB failed to return a valid ID
        /// </exception>
        public async Task<long> Insert(PsbNamedAccount acc) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO psb_named (
                    tag, name, player_name,
                    vs_id, nc_id, tr_id, ns_id,
                    vs_status, nc_status, tr_status, ns_status
                ) VALUES (
                    @Tag, @Name, @PlayerName,
                    @VsID, @NcID, @TrID, @NsID,
                    @VsStatus, @NcStatus, @TrStatus, @NsStatus
                ) RETURNING id;
            ");

            cmd.AddParameter("Tag", acc.Tag);
            cmd.AddParameter("Name", acc.Name);
            cmd.AddParameter("PlayerName", acc.PlayerName);

            cmd.AddParameter("VsID", acc.VsID);
            cmd.AddParameter("NcID", acc.NcID);
            cmd.AddParameter("TrID", acc.TrID);
            cmd.AddParameter("NsID", acc.NsID);

            cmd.AddParameter("VsStatus", acc.VsStatus);
            cmd.AddParameter("NcStatus", acc.NcStatus);
            cmd.AddParameter("TrStatus", acc.TrStatus);
            cmd.AddParameter("NsStatus", acc.NsStatus);

            object? objID = await cmd.ExecuteScalarAsync();
            await conn.CloseAsync();

            if (objID != null && long.TryParse(objID.ToString(), out long ID) == true) {
                return ID;
            } else {
                throw new Exception($"Missing or bad type on 'id': {objID} {objID?.GetType()}");
            }
        }

        /// <summary>
        ///     Update an existing entry by ID
        /// </summary>
        /// <param name="ID">ID of the account to update</param>
        /// <param name="acc">Parameters used to update</param>
        /// <returns></returns>
        public async Task UpdateByID(long ID, PsbNamedAccount acc) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE psb_named 
                    SET tag = @Tag,
                        name = @Name,
                        player_name = @PlayerName,
                        vs_id = @VsID,
                        nc_id = @NcID,
                        tr_id = @TrID,
                        ns_id = @NsID,
                        vs_status = @VsStatus,
                        nc_status = @NcStatus,
                        tr_status = @TrStatus,
                        ns_status = @NsStatus
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);
            cmd.AddParameter("Tag", acc.Tag);
            cmd.AddParameter("Name", acc.Name);
            cmd.AddParameter("PlayerName", acc.PlayerName);

            cmd.AddParameter("VsID", acc.VsID);
            cmd.AddParameter("NcID", acc.NcID);
            cmd.AddParameter("TrID", acc.TrID);
            cmd.AddParameter("NsID", acc.NsID);

            cmd.AddParameter("VsStatus", acc.VsStatus);
            cmd.AddParameter("NcStatus", acc.NcStatus);
            cmd.AddParameter("TrStatus", acc.TrStatus);
            cmd.AddParameter("NsStatus", acc.NsStatus);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
