using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class CharacterDirectiveTierReader : IDataReader<CharacterDirectiveTier> {

        public override CharacterDirectiveTier? ReadEntry(NpgsqlDataReader reader) {
            CharacterDirectiveTier dir = new CharacterDirectiveTier();
            
            dir.CharacterID = reader.GetString("character_id");
            dir.TierID = reader.GetInt32("tier_id");
            dir.TreeID = reader.GetInt32("tree_id");
            dir.CompletionDate = reader.GetNullableDateTime("completion_date");

            return dir;
        }

    }
}
