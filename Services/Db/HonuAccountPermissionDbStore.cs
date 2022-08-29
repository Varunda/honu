using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db {

    public class HonuAccountPermissionDbStore {

        private readonly ILogger<HonuAccountPermissionDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<HonuAccountPermission> _Reader;

        public HonuAccountPermissionDbStore(ILogger<HonuAccountPermissionDbStore> logger,
            IDbHelper dbHelper, IDataReader<HonuAccountPermission> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a single account permission by its ID
        /// </summary>
        /// <param name="ID">ID of the specific permission to get</param>
        public async Task<HonuAccountPermission?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account_permission
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            HonuAccountPermission? perm = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return perm;
        }

        /// <summary>
        ///     Get the account permissions of an account
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        public async Task<List<HonuAccountPermission>> GetByAccountID(long accountID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account_permission
                    WHERE account_id = @AccountID;
            ");

            cmd.AddParameter("AccountID", accountID);

            List<HonuAccountPermission> perms = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return perms;
        }

        /// <summary>
        ///     Insert a new <see cref="HonuAccountPermission"/>
        /// </summary>
        /// <param name="perm">Parameters used to insert</param>
        /// <returns>The ID the row was given in the table</returns>
        /// <exception cref="ArgumentException">If one of the fields in <paramref name="perm"/> was invalid</exception>
        public async Task<long> Insert(HonuAccountPermission perm) {
            if (string.IsNullOrWhiteSpace(perm.Permission)) {
                throw new ArgumentException($"Passed permission has a {nameof(HonuAccountPermission.Permission)} that is null or whitespace");
            }
            if (perm.GrantedByID <= 0) {
                throw new ArgumentException($"Passed permission has a {nameof(HonuAccountPermission.GrantedByID)} that is 0 or lower");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO honu_account_permission (
                    account_id, permission, timestamp, granted_by_id
                ) VALUES (
                    @AccountID, @Permission, @Timestamp, @GrantedByID 
                ) RETURNING id;
            ");

            cmd.AddParameter("AccountID", perm.AccountID);
            cmd.AddParameter("Permission", perm.Permission);
            cmd.AddParameter("Timestamp", DateTime.UtcNow);
            cmd.AddParameter("GrantedByID", perm.GrantedByID);

            long id = await cmd.ExecuteInt64(CancellationToken.None);

            return id;
        }

        /// <summary>
        ///     Delete a specific <see cref="HonuAccountPermission"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the permission to delete</param>
        public async Task DeleteByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE 
                    FROM honu_account_permission
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
