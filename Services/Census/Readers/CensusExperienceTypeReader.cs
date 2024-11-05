using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusExperienceTypeReader : ICensusReader<ExperienceType> {
        public CensusExperienceTypeReader(CensusMetric metrics) : base(metrics) {
        }

        public override ExperienceType? ReadEntry(JsonElement token) {
            ExperienceType type = new ExperienceType();

            type.ID = token.GetRequiredInt32("experience_id");
            type.Name = token.GetString("description", "");
            type.Amount = (double) token.GetDecimal("xp", 0);
            type.AwardTypeID = token.GetInt32("experience_award_type_id", 0);

            return type;
        }

    }
}
