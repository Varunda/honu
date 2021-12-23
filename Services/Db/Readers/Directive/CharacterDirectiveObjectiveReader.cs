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

    public class CharacterDirectiveObjectiveReader : IDataReader<CharacterDirectiveObjective> {

        public override CharacterDirectiveObjective? ReadEntry(NpgsqlDataReader reader) {
            CharacterDirectiveObjective dir = new CharacterDirectiveObjective();
            
            dir.CharacterID = reader.GetString("character_id");
            dir.DirectiveID = reader.GetInt32("directive_id");
            dir.ObjectiveID = reader.GetInt32("objective_id");
            dir.ObjectiveGroupID = reader.GetInt32("objective_group_id");
            dir.Status = reader.GetInt32("status");
            dir.StateData = reader.GetInt32("state_data");

            return dir;
        }

    }
}
