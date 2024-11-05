using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterDirectiveObjectiveReader : ICensusReader<CharacterDirectiveObjective> {
        public CensusCharacterDirectiveObjectiveReader(CensusMetric metrics) : base(metrics) {
        }

        public override CharacterDirectiveObjective? ReadEntry(JsonElement token) {
            CharacterDirectiveObjective dir = new CharacterDirectiveObjective();

            dir.CharacterID = token.GetRequiredString("character_id");
            dir.DirectiveID = token.GetInt32("directive_id", 0);
            dir.ObjectiveID = token.GetInt32("objective_id", 0);
            dir.ObjectiveGroupID = token.GetInt32("objective_group_id", 0);
            dir.Status = token.GetInt32("directive_status", 0);
            dir.StateData = token.GetInt32("state_data", 0);

            return dir;
        }

    }
}
