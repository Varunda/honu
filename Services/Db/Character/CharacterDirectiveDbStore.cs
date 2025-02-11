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

        /// <summary>
        ///     Get the directives of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        public async Task<List<CharacterDirective>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_directives
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterDirective> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            // if there are no dirs in the character DB, see if we can rescue them from the events DB
            if (dirs.Count == 0) {
                _Logger.LogDebug($"checking for character directives in event db [charID={charID}]");
                using NpgsqlConnection conn2 = _DbHelper.Connection(Dbs.EVENTS);
                using NpgsqlCommand cmd2 = await _DbHelper.Command(conn2, @"
                    SELECT *
                        FROM character_directives
                        WHERE character_id = @CharacterID;
                ");

                cmd2.AddParameter("CharacterID", charID);

                dirs = await _Reader.ReadList(cmd2);

                // save them back to the character DB
                if (dirs.Count > 0) {
                    _Logger.LogDebug($"found {dirs.Count} stats from events DB, putting back into character DB");
                    await UpsertMany(charID, dirs);
                } else {
                    _Logger.LogDebug($"found 0 directives from events DB, probably never had any");
                }
            }

            return dirs;
        }

        /// <summary>
        ///     Upsert a single <see cref="CharacterDirective"/>
        /// </summary>
        /// <param name="charID">ID of the character being updated</param>
        /// <param name="dir">Parameters used to upsert</param>
        public async Task Upsert(string charID, CharacterDirective dir) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
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
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        /// <summary>
        ///     Upsert (update/insert) many entries at once, saving potentially thousands of DB calls when updating characters
        /// </summary>
        /// <remarks>
        ///     This works by deleting all directive information that is passed, which means data that doesn't exist in Census,
        ///     will NOT be deleted, which is good cause I don't like deleting data
        /// </remarks>
        /// <param name="charID">ID of the character</param>
        /// <param name="dirs">Directives being updated</param>
        public async Task UpsertMany(string charID, List<CharacterDirective> dirs) {
            if (dirs.Count == 0) {
                return;
            }

            List<int> existingIDs = dirs.Select(iter => iter.DirectiveID).ToList();

            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                BEGIN;

                DELETE FROM character_directives
                    WHERE character_id = @CharacterID
                        AND directive_id IN ({string.Join(",", existingIDs)});

                INSERT INTO character_directives (
                    character_id, directive_id, directive_tree_id, completion_date
                ) VALUES    
                    {string.Join(",", dirs.Select((iter, index) => $"(@CharacterID, @DirectiveID_{index}, @DirectiveTreeID_{index}, @CompletionDate_{index})\n"))}
                ;

                COMMIT;
            ");


            cmd.AddParameter("ExistingIDs", existingIDs);
            cmd.AddParameter("CharacterID", charID);

            for (int i = 0; i < dirs.Count; ++i) {
                CharacterDirective dir = dirs[i];

                cmd.AddParameter($"DirectiveID_{i}", dir.DirectiveID);
                cmd.AddParameter($"DirectiveTreeID_{i}", dir.TreeID);
                cmd.AddParameter($"CompletionDate_{i}", dir.CompletionDate);
            }

            //_Logger.LogDebug(cmd.Print());

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}