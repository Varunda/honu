using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class CharacterNameChangeDbStore {

        private readonly ILogger<CharacterNameChangeDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterNameChange> _Reader;

        public CharacterNameChangeDbStore(ILogger<CharacterNameChangeDbStore> logger,
            IDbHelper dbHelper, IDataReader<CharacterNameChange> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     insert a new <see cref="CharacterNameChange"/> into the DB
        /// </summary>
        /// <param name="change">parameters used to insert</param>
        /// <returns>
        ///     a task for when the async operation is complete
        /// </returns>
        public async Task Insert(CharacterNameChange change) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_name_change (
                    character_id, old_name, new_name, lower_bound, upper_bound, timestamp
                ) VALUES (
                    @CharacterID, @OldName, @NewName, @LowerBound, @UpperBound, @Timestamp
                );
            ");

            cmd.AddParameter("CharacterID", change.CharacterID);
            cmd.AddParameter("OldName", change.OldName);
            cmd.AddParameter("NewName", change.NewName);
            cmd.AddParameter("LowerBound", change.LowerBound);
            cmd.AddParameter("UpperBound", change.UpperBound);
            cmd.AddParameter("Timestamp", change.Timestamp);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     get a list of all character name changes a character has had
        /// </summary>
        /// <param name="charID">ID of the character to get the name changes of</param>
        /// <returns>
        ///     a list of <see cref="CharacterNameChange"/>s with 
        ///     <see cref="CharacterNameChange.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        public async Task<List<CharacterNameChange>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_name_change
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterNameChange> changes = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return changes;
        }

    }
}
