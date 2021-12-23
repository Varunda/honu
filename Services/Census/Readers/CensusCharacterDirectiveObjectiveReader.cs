using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterDirectiveObjectiveReader : ICensusReader<CharacterDirectiveObjective> {

        public override CharacterDirectiveObjective? ReadEntry(JToken token) {
            CharacterDirectiveObjective dir = new CharacterDirectiveObjective();

            dir.CharacterID = token.GetRequiredString("character_id");
            dir.DirectiveID = token.GetInt32("directive_id", 0);
            dir.ObjectiveID = token.GetInt32("directive_tree_id", 0);
            dir.ObjectiveGroupID = token.GetInt32("directive_tree_group_id", 0);
            dir.Status = token.GetInt32("directive_status", 0);
            dir.StateData = token.GetInt32("state_data", 0);

            return dir;
        }

    }
}
