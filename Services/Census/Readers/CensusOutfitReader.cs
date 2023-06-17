using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusOutfitReader : ICensusReader<PsOutfit> {

        public override PsOutfit? ReadEntry(JsonElement token) {
            PsOutfit outfit = new PsOutfit();

            outfit.ID = token.GetRequiredString("outfit_id");
            outfit.Name = token.GetString("name", "<MISSING NAME>");
            outfit.Tag = token.NullableString("alias");
            outfit.MemberCount = token.GetInt32("member_count", 0);
            outfit.LeaderID = token.GetString("leader_character_id", "");
            outfit.DateCreated = token.CensusTimestamp("time_created");
            outfit.FactionID = token.GetChild("leader")?.GetInt16("faction_id", -1) ?? -1;

            return outfit;
        }

    }
}
