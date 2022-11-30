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
            /*
                WITH char_ids AS (
                    SELECT id, vs_id AS ids FROM psb_named WHERE vs_id IS NOT NULL
                    UNION SELECT id, nc_id AS ids FROM psb_named WHERE nc_id IS NOT NULL
                    UNION SELECT id, tr_id AS ids FROM psb_named WHERE tr_id IS NOT NULL
                    UNION SELECT id, ns_id AS ids FROM psb_named WHERE ns_id IS NOT NULL
                ), times AS (
                    SELECT ci.id, SUM(EXTRACT(epoch FROM s.finish - s.start)) AS seconds_online
                        FROM wt_session s
                            INNER JOIN char_ids ci ON s.character_id = ci.ids
                        WHERE character_id IN (SELECT ids FROM char_ids)
                            AND start >= (NOW() AT TIME ZONE 'utc' - '90 days'::INTERVAL)
                        GROUP BY ci.id
                )
                SELECT *
                    FROM psb_named pn1
                    LEFT JOIN times t ON pn1.id = t.id order by pn1.id ASC;

                SELECT pn1.*, usage.seconds_online
                    FROM psb_named pn1
                    LEFT JOIN (
                        SELECT pn2.id, SUM(EXTRACT(epoch FROM s.finish - s.start)) AS seconds_online
                            FROM psb_named pn2
                            LEFT JOIN wt_session s ON s.character_id = pn2.vs_id OR s.character_id = pn2.nc_id OR s.character_id = pn2.tr_id
                            WHERE s.start >= (NOW() AT TIME ZONE 'utc' - '90 days'::INTERVAL)
                            GROUP BY pn2.id
                    ) usage ON usage.id = pn1.id
                    ORDER BY pn1.ID ASC;
             */


            List<PsbNamedAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        /// <summary>
        ///     Get a single named account by ID
        /// </summary>
        /// <param name="ID">ID of the account to get</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if the account doesn't exist
        /// </returns>
        public async Task<PsbNamedAccount?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT pn1.*
                    FROM psb_named pn1
                    WHERE pn1.id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsbNamedAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        /// <summary>
        ///     Get the named account that has the tag and name (case sensitive)
        /// </summary>
        /// <param name="tag">Tag of the psb account</param>
        /// <param name="name">Name of the psb account</param>
        /// <returns>
        ///     The <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.Tag"/> of <paramref name="tag"/>,
        ///     and <see cref="PsbNamedAccount"/> with <see cref="PsbNamedAccount.Name"/> of <paramref name="name"/>
        /// </returns>
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
        ///     Get how many seconds the characters on a <see cref="PsbNamedAccount"/> have played
        ///     in the last 3 months
        /// </summary>
        /// <param name="accountID">ID of the <see cref="PsbNamedAccount"/></param>
        /// <returns>
        ///     How many seconds each character of the <see cref="PsbNamedAccount"/> has played
        ///     in the last 90 days
        /// </returns>
        public async Task<long> GetPlaytime(long accountID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT COALESCE(usage.seconds_online, 0) AS play_time
                    FROM psb_named pn1
                    LEFT JOIN (
                        SELECT pn2.id, SUM(EXTRACT(epoch FROM s.finish - s.start)) AS seconds_online
                            FROM psb_named pn2
                            LEFT JOIN wt_session s ON s.character_id = pn2.vs_id OR s.character_id = pn2.nc_id OR s.character_id = pn2.tr_id
                            WHERE s.start >= (NOW() AT TIME ZONE 'utc' - '90 days'::INTERVAL)
                            GROUP BY pn2.id
                    ) usage ON usage.id = pn1.id
                    WHERE pn1.id = @AccountID;
            ");

            cmd.AddParameter("AccountID", accountID);

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
        public async Task UpdateByID(long ID, PsbNamedAccount acc) {
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
