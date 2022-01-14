using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db {

    public class PsbAccountNoteDbStore {

        private readonly ILogger<PsbAccountNoteDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsbAccountNote> _Reader;

        public PsbAccountNoteDbStore(ILogger<PsbAccountNoteDbStore> logger,
            IDbHelper helper, IDataReader<PsbAccountNote> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get a <see cref="PsbAccountNote"/> by its ID
        /// </summary>
        /// <param name="ID">ID of the account to get</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     The <see cref="PsbAccountNote"/> with <see cref="PsbAccountNote.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<PsbAccountNote?> GetByID(long ID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_account_note
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            return await cmd.ExecuteReadSingle(_Reader, cancel);
        }

        /// <summary>
        ///     Get all notes of an account
        /// </summary>
        /// <param name="accountID">ID of the account</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     All <see cref="PsbAccountNote"/>s with <see cref="PsbAccountNote.AccountID"/> of <paramref name="accountID"/>
        /// </returns>
        public async Task<List<PsbAccountNote>> GetByAccountID(long accountID, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_account_note
                    WHERE account_id = @AccountID;
            ");

            cmd.AddParameter("AccountID", accountID);

            return await cmd.ExecuteReadList(_Reader, cancel);
        }

        /// <summary>
        ///     Insert a new note on an account
        /// </summary>
        /// <param name="accountID">ID of the account the note is for</param>
        /// <param name="note">Parameters used to insert the note</param>
        /// <param name="cancel">Cancellation token</param>
        public async Task<long> Insert(long accountID, PsbAccountNote note, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO psb_account_note (
                    account_id, honu_id, timestamp, message
                ) VALUES (
                    @AccountID, @HonuID, NOW() AT TIME ZONE 'utc', @Message
                ) RETURNING id;
            ");

            cmd.AddParameter("AccountID", accountID);
            cmd.AddParameter("HonuID", note.HonuID);
            cmd.AddParameter("Message", note.Message);

            return await cmd.ExecuteInt64(cancel);
        }

    }
}
