using Microsoft.Extensions.Logging;
using Npgsql;
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
        ///     Get a <see cref="HonuAccount"/> by it's email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
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
        ///     Insert a new <see cref="HonuAccount"/>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
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

    }
}
