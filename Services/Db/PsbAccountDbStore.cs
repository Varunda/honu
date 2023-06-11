using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;
using watchtower.Models.PSB;

namespace watchtower.Services.Db {

    /// <summary>
    ///     DB service to interact with psb accounts
    /// </summary>
    public class PsbAccountDbStore {

        private readonly ILogger<PsbAccountDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsbAccount> _Reader;

        public PsbAccountDbStore(ILogger<PsbAccountDbStore> logger,
            IDbHelper helper, IDataReader<PsbAccount> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get all psb accounts
        /// </summary>
        public async Task<List<PsbAccount>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named;
            ");

            List<PsbAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        /// <summary>
        ///     Get all <see cref="PsbAccount"/>s with <see cref="PsbAccount.AccountType"/> of <paramref name="typeID"/>
        /// </summary>
        /// <param name="typeID">ID of the account type to get</param>
        /// <returns></returns>
        public async Task<List<PsbAccount>> GetByTypeID(long typeID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE account_type = @TypeID;
            ");

            cmd.AddParameter("TypeID", typeID);

            List<PsbAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        /// <summary>
        ///     Get a single psb account by ID
        /// </summary>
        /// <param name="ID">ID of the account to get</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if the account doesn't exist
        /// </returns>
        public async Task<PsbAccount?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT pn1.*
                    FROM psb_named pn1
                    WHERE pn1.id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsbAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        /// <summary>
        ///     Get the psb account that has the tag and name (case sensitive)
        /// </summary>
        /// <param name="tag">Tag of the psb account</param>
        /// <param name="name">Name of the psb account</param>
        /// <returns>
        ///     The <see cref="PsbAccount"/> with <see cref="PsbAccount.Tag"/> of <paramref name="tag"/>,
        ///     and <see cref="PsbAccount"/> with <see cref="PsbAccount.Name"/> of <paramref name="name"/>
        /// </returns>
        public async Task<PsbAccount?> GetByTagAndName(string? tag, string name) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE tag = @Tag
                        AND name = @Name;
            ");

            cmd.AddParameter("Tag", tag);
            cmd.AddParameter("Name", name);

            PsbAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        /// <summary>
        ///     Get how many seconds the characters on a <see cref="PsbAccount"/> have played
        ///     in the last 3 months
        /// </summary>
        /// <param name="accountID">ID of the <see cref="PsbAccount"/></param>
        /// <returns>
        ///     How many seconds each character of the <see cref="PsbAccount"/> has played
        ///     in the last 90 days
        /// </returns>
        public async Task<long> GetPlaytime(long accountID) {
            PsbAccount? account = await GetByID(accountID);
            if (account == null) {
                return 0;
            }

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT COALESCE(usage.seconds_online, 0) AS play_time
                    FROM (
                        SELECT SUM(EXTRACT(epoch FROM s.finish - s.start)) AS seconds_online
                            FROM wt_session s
                            WHERE (s.character_id = @VsID OR s.character_id = @NcID OR s.character_id = @TrID)
                                AND (s.start >= (NOW() AT TIME ZONE 'utc' - '90 days'::INTERVAL))
                    ) usage
            ");

            cmd.AddParameter("VsID", account.VsID);
            cmd.AddParameter("NcID", account.NcID);
            cmd.AddParameter("TrID", account.TrID);

            object? playTime = await cmd.ExecuteScalarAsync(CancellationToken.None);

            if (playTime == null) {
                return 0;
            }

            return Convert.ToInt64(playTime);
        }

        /// <summary>
        ///     Mark a named account as deleted. Does not actually delete from DB
        /// </summary>
        /// <param name="ID">ID of the account to mark as deleted</param>
        /// <param name="deletedByID">ID of the <see cref="HonuAccount"/> that is marking the account as deleted</param>
        /// <returns>A task when the async operation is complete</returns>
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
        ///     Insert a new <see cref="PsbAccount"/>
        /// </summary>
        /// <param name="acc">Parameters used to insert</param>
        /// <returns>
        ///     The ID of the row just inserted
        /// </returns>
        /// <exception cref="Exception">
        ///     If the DB failed to return a valid ID
        /// </exception>
        public async Task<long> Insert(PsbAccount acc) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO psb_named (
                    account_type,
                    tag, name, player_name,
                    vs_id, nc_id, tr_id, ns_id,
                    vs_status, nc_status, tr_status, ns_status,
                    play_time
                ) VALUES (
                    @AccountType,
                    @Tag, @Name, @PlayerName,
                    @VsID, @NcID, @TrID, @NsID,
                    @VsStatus, @NcStatus, @TrStatus, @NsStatus,
                    0
                ) RETURNING id;
            ");

            cmd.AddParameter("AccountType", acc.AccountType);
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

            long ID = await cmd.ExecuteInt64(CancellationToken.None);
            return ID;
        }

        /// <summary>
        ///     Update an existing entry by ID
        /// </summary>
        /// <param name="ID">ID of the account to update</param>
        /// <param name="acc">Parameters used to update</param>
        /// <returns></returns>
        public async Task UpdateByID(long ID, PsbAccount acc) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE psb_named 
                    SET account_type = @AccountType,
                        tag = @Tag,
                        name = @Name,
                        player_name = @PlayerName,
                        vs_id = @VsID,
                        nc_id = @NcID,
                        tr_id = @TrID,
                        ns_id = @NsID,
                        vs_status = @VsStatus,
                        nc_status = @NcStatus,
                        tr_status = @TrStatus,
                        ns_status = @NsStatus,
                        play_time = @PlayTime
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            cmd.AddParameter("AccountType", acc.AccountType);
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

            cmd.AddParameter("PlayTime", acc.SecondsUsage);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
