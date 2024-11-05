using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterDirectiveReader : ICensusReader<CharacterDirective> {
        public CensusCharacterDirectiveReader(CensusMetric metrics) : base(metrics) {
        }

        public override CharacterDirective? ReadEntry(JsonElement token) {
            CharacterDirective dir = new CharacterDirective();

            dir.CharacterID = token.GetRequiredString("character_id");
            dir.DirectiveID = token.GetRequiredInt32("directive_id");
            dir.TreeID = token.GetInt32("directive_tree_id", 0);
            dir.CompletionDate = token.GetInt32("completion_time", 0) == 0 ? null : token.CensusTimestamp("completion_time");

            return dir;
        }

    }
}
