using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterDirectiveTreeDbStore {

        private readonly ILogger<CharacterDirectiveTreeDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterDirectiveTree> _Reader;

        public CharacterDirectiveTreeDbStore(ILogger<CharacterDirectiveTreeDbStore> logger,
            IDbHelper helper, IDataReader<CharacterDirectiveTree> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<CharacterDirectiveTree>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_directive_tree
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterDirectiveTree> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(string charID, CharacterDirectiveTree dir) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_directive_tree (
                    character_id, tree_id, current_tier, current_level, completion_date
                ) VALUES (
                    @CharacterID, @TreeID, @CurrentTier, @CurrentLevel, @CompletionDate
                ) ON CONFLICT (character_id, tree_id) DO UPDATE
                    SET current_tier = @CurrentTier,
                        current_level = @CurrentLevel,
                        completion_date = @CompletionDate;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("TreeID", dir.TreeID);
            cmd.AddParameter("CurrentTier", dir.CurrentTier);
            cmd.AddParameter("CurrentLevel", dir.CurrentLevel);
            cmd.AddParameter("CompletionDate", dir.CompletionDate);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}