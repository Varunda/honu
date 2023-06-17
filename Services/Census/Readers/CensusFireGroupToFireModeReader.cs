using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusFireGroupToFireModeReader : ICensusReader<FireGroupToFireMode> {

        public override FireGroupToFireMode? ReadEntry(JsonElement token) {
            FireGroupToFireMode mode = new();

            mode.FireGroupID = token.GetRequiredInt32("fire_group_id");
            mode.FireModeID = token.GetRequiredInt32("fire_mode_id");
            mode.FireModeIndex = token.GetRequiredInt32("fire_mode_index");

            return mode;
        }

    }
}
