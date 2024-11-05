using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusOutfitWarsMatchReader : ICensusReader<OutfitWarsMatch> {
        public CensusOutfitWarsMatchReader(CensusMetric metrics) : base(metrics) {
        }

        public override OutfitWarsMatch? ReadEntry(JsonElement token) {
            OutfitWarsMatch match = new();

            match.MatchID = token.GetRequiredString("match_id");
            match.RoundID = token.GetString("round_id", "<missing>");
            match.OutfitWarID = token.GetInt32("outfit_war_id", -1);
            match.OutfitAId = token.GetString("outfit_a_id", "0");
            match.OutfitBId = token.GetString("outfit_b_id", "0");
            match.Timestamp = token.CensusTimestamp("start_time");
            match.WorldID = token.GetInt16("world_id", -1);
            match.OutfitAFactionId = token.GetInt16("outfit_a_faction_id", -1);
            match.OutfitBFactionId = token.GetInt16("outfit_b_faction_id", -1);

            return match;
        }

    }
}
