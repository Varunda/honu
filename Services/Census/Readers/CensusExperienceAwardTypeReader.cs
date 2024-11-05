using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusExperienceAwardTypeReader : ICensusReader<ExperienceAwardType> {
        public CensusExperienceAwardTypeReader(CensusMetric metrics) : base(metrics) {
        }

        public override ExperienceAwardType? ReadEntry(JsonElement token) {
            ExperienceAwardType type = new();

            type.ID = token.GetRequiredInt32("experience_award_type_id");
            type.Name = token.GetString("name", "<missing name>");

            return type;
        }

    }
}
