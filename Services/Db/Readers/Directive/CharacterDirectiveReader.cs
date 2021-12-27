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

    public class CharacterDirectiveReader : IDataReader<CharacterDirective> {

        public override CharacterDirective? ReadEntry(NpgsqlDataReader reader) {
            CharacterDirective dir = new CharacterDirective();
            
            dir.CharacterID = reader.GetString("character_id");
            dir.DirectiveID = reader.GetInt32("directive_id");
            dir.TreeID = reader.GetInt32("directive_tree_id");
            dir.CompletionDate = reader.GetNullableDateTime("completion_date");

            return dir;
        }

    }
}
