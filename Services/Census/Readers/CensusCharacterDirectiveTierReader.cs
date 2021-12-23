using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterDirectiveTierReader : ICensusReader<CharacterDirectiveTier> {

        public override CharacterDirectiveTier? ReadEntry(JToken token) {
            CharacterDirectiveTier dir = new CharacterDirectiveTier();

            dir.CharacterID = token.GetRequiredString("character_id");
            dir.TreeID = token.GetInt32("directive_tree_id", 0);
            dir.TierID = token.GetInt32("directive_tier_id", 0);
            dir.CompletionDate = token.GetInt32("completion_time", 0) == 0 ? null : token.CensusTimestamp("completion_time");

            return dir;
        }

    }
}
