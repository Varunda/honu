using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models;

namespace watchtower.Services.Db {

    public class HonuAccountAccessLogDbStore {

        private readonly ILogger<HonuAccountAccessLogDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        
        public HonuAccountAccessLogDbStore(ILogger<HonuAccountAccessLogDbStore> logger,
            IDbHelper helper) {

            _Logger = logger;
            _DbHelper = helper;
        }

        /// <summary>
        ///     Insert a new <see cref="HonuAccountAccessLog"/> to the DB
        /// </summary>
        /// <param name="log">Parameters used to insert</param>
        /// <exception cref="ArgumentException">
        ///     Throw if both <see cref="HonuAccountAccessLog.HonuID"/> and <see cref="HonuAccountAccessLog.Email"/> is null
        /// </exception>
        public async Task Insert(HonuAccountAccessLog log) {
            if (log.Email == null && log.HonuID == null) {
                throw new ArgumentException($"Both Email and HonuID cannot be null");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO honu_account_access_logs (
                    timestamp, success, honu_id, email
                ) VALUES (
                    NOW() AT TIME ZONE 'utc', @Success, @HonuID, @Email
                );
            ");

            cmd.AddParameter("Success", log.Success);
            cmd.AddParameter("HonuID", log.HonuID);
            cmd.AddParameter("Email", log.Email);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
