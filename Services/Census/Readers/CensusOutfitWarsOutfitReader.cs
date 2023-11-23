using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusOutfitWarsOutfitReader : ICensusReader<OutfitWarsOutfit> {
        public override OutfitWarsOutfit? ReadEntry(JsonElement token) {
            OutfitWarsOutfit o = new();

            o.OutfitID = token.GetRequiredString("outfit_id");
            o.FactionID = token.GetInt16("faction_id", -1);
            o.WorldID = token.GetInt16("world_id", -1);
            o.OutfitWarID = token.GetInt32("outfit_war_id", -1);
            o.RegistrationOrder = token.GetInt32("registration_order", -1);
            o.Status = token.GetString("status", "<unknown>");
            o.SignupCount = token.GetInt32("member_signup_count", -1);

            return o;
        }
    }
}
