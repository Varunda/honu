using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterEventGroupedReader : ICensusReader<CharacterEventGrouped> {

        public override CharacterEventGrouped? ReadEntry(JsonElement token) {
            CharacterEventGrouped group = new();

            group.CharacterID = token.GetRequiredString("character_id");
            group.TableType = token.GetRequiredString("table_type");
            group.Count = token.GetRequiredInt32("count");

            return group;
        }

    }
}
