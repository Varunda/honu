using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterDirectiveDbStore {

        private readonly ILogger<CharacterDirectiveDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterDirective> _Reader;

        public CharacterDirectiveDbStore(ILogger<CharacterDirectiveDbStore> logger,
            IDbHelper helper, IDataReader<CharacterDirective> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<CharacterDirective>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_directives
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterDirective> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(string charID, CharacterDirective dir) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_directives (
                    character_id, directive_id, directive_tree_id, completion_date
                ) VALUES (
                    @CharacterID, @DirectiveID, @DirectiveTreeID, @CompletionDate
                ) ON CONFLICT (character_id, directive_id) DO UPDATE
                    SET directive_tree_id = @DirectiveTreeID,
                        completion_date = @CompletionDate;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("DirectiveID", dir.DirectiveID);
            cmd.AddParameter("DirectiveTreeID", dir.TreeID);
            cmd.AddParameter("CompletionDate", dir.CompletionDate);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}