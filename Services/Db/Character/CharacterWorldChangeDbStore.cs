
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Character {

    public class CharacterWorldChangeDbStore {

        private readonly ILogger<CharacterWorldChangeDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<WorldChange> _Reader;

        public CharacterWorldChangeDbStore(ILogger<CharacterWorldChangeDbStore> logger,
            IDbHelper dbHelper, IDataReader<WorldChange> reader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     get all <see cref="WorldChange"/> based on character ID
        /// </summary>
        /// <param name="charID">ID of the character to get the changes of</param>
        /// <returns></returns>
        public async Task<List<WorldChange>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM world_changes
                    WHERE character_id = @CharID; 
            ");

            cmd.AddParameter("CharID", charID);
            await cmd.PrepareAsync();

            List<WorldChange> changes = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return changes;
        }

    }
}
