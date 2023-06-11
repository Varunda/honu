using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterDirectiveTierDbStore {

        private readonly ILogger<CharacterDirectiveTierDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterDirectiveTier> _Reader;

        public CharacterDirectiveTierDbStore(ILogger<CharacterDirectiveTierDbStore> logger,
            IDbHelper helper, IDataReader<CharacterDirectiveTier> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<CharacterDirectiveTier>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_directive_tier
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterDirectiveTier> dirs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return dirs;
        }

        public async Task Upsert(string charID, CharacterDirectiveTier dir) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_directive_tier (
                    character_id, tree_id, tier_id, completion_date
                ) VALUES (
                    @CharacterID, @TreeID, @TierID, @CompletionDate
                ) ON CONFLICT (character_id, tree_id, tier_id) DO UPDATE
                    SET completion_date = @CompletionDate;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("TreeID", dir.TreeID);
            cmd.AddParameter("TierID", dir.TierID);
            cmd.AddParameter("CompletionDate", dir.CompletionDate);
            await cmd.PrepareAsync();

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}