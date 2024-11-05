using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterDirectiveTreeReader : ICensusReader<CharacterDirectiveTree> {
        public CensusCharacterDirectiveTreeReader(CensusMetric metrics) : base(metrics) {
        }

        public override CharacterDirectiveTree? ReadEntry(JsonElement token) {
            CharacterDirectiveTree dir = new CharacterDirectiveTree();

            dir.CharacterID = token.GetRequiredString("character_id");
            dir.TreeID = token.GetInt32("directive_tree_id", 0);
            dir.CurrentTier = token.GetInt32("current_directive_tier_id", 0);
            dir.CurrentLevel = token.GetInt32("current_level", 0);
            dir.CompletionDate = token.GetInt32("completion_time", 0) == 0 ? null : token.CensusTimestamp("completion_time");

            return dir;
        }

    }
}
