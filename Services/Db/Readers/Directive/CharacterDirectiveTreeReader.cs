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

    public class CharacterDirectiveTreeReader : IDataReader<CharacterDirectiveTree> {

        public override CharacterDirectiveTree? ReadEntry(NpgsqlDataReader reader) {
            CharacterDirectiveTree dir = new CharacterDirectiveTree();
            
            dir.CharacterID = reader.GetString("character_id");
            dir.TreeID = reader.GetInt32("tree_id");
            dir.CurrentTier = reader.GetInt32("current_tier");
            dir.CurrentLevel = reader.GetInt32("current_level");
            dir.CompletionDate = reader.GetNullableDateTime("completion_date");

            return dir;
        }

    }
}
