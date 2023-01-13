using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db {

    public class HonuAccountDbStore {

        private readonly ILogger<HonuAccountDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<HonuAccount> _Reader;

        public HonuAccountDbStore(ILogger<HonuAccountDbStore> logger,
            IDbHelper helper, IDataReader<HonuAccount> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a specific <see cref="HonuAccount"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the account to get</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The <see cref="HonuAccount"/> with <see cref="HonuAccount.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<HonuAccount?> GetByID(long ID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            return await cmd.ExecuteReadSingle(_Reader, cancel);
        }

        /// <summary>
        ///     Get all <see cref="HonuAccount"/>s
        /// </summary>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     A list of all <see cref="HonuAccount"/>s, including ones that are deactivated
        /// </returns>
        public async Task<List<HonuAccount>> GetAll(CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account;
            ");

            return await cmd.ExecuteReadList(_Reader, cancel);
        }

        /// <summary>
        ///     Get a <see cref="HonuAccount"/> by <see cref="HonuAccount.Email"/>
        /// </summary>
        /// <param name="email">Email to get the account of</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The <see cref="HonuAccount"/> with <see cref="HonuAccount.Email"/> of <paramref name="email"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<HonuAccount?> GetByEmail(string email, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account
                    WHERE email = @Email;
            ");

            cmd.AddParameter("Email", email);

            return await cmd.ExecuteReadSingle(_Reader, cancel);
        }

        /// <summary>
        ///     Get a <see cref="HonuAccount"/> by <see cref="HonuAccount.DiscordID"/>
        /// </summary>
        /// <param name="discordID">ID of the discord account to get the honu account of</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The <see cref="HonuAccount"/> with <see cref="HonuAccount.DiscordID"/> of <paramref name="discordID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<HonuAccount?> GetByDiscordID(ulong discordID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM honu_account
                    WHERE discord_id = @DiscordID;
            ");

            cmd.AddParameter("DiscordID", discordID);

            return await cmd.ExecuteReadSingle(_Reader, cancel);
        }

        /// <summary>
        ///     Insert a new <see cref="HonuAccount"/>
        /// </summary>
        /// <param name="param">Parameters used to insert the new account</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The <see cref="HonuAccount.ID"/> of the row that was newly inserted in the DB
        /// </returns>
        public async Task<long> Insert(HonuAccount param, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO honu_account (
                    name, email, discord, discord_id, timestamp 
                ) VALUES (
                    @Name, @Email, @Discord, @DiscordID, NOW() AT TIME ZONE 'utc'
                ) RETURNING id;
            ");

            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Email", param.Email);
            cmd.AddParameter("Discord", param.Discord);
            cmd.AddParameter("DiscordID", param.DiscordID);

            return await cmd.ExecuteInt64(cancel);
        }

        /// <summary>
        ///     Delete an account, marking it as deactive
        /// </summary>
        /// <param name="accountID">ID of the account to delete</param>
        /// <param name="deletedByID">ID of the account that is performing the delete</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     When the operation has completed
        /// </returns>
        public async Task Delete(long accountID, long deletedByID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                UPDATE honu_account
                    SET deleted_on = NOW() AT TIME ZONE 'utc',
                        deleted_by = @DeletedByID
                    WHERE id = @ID;
            ");

            cmd.AddParameter("DeletedByID", deletedByID);
            cmd.AddParameter("ID", accountID);

            await cmd.ExecuteNonQueryAsync(cancel);
            await conn.CloseAsync();
        }

    }
}
