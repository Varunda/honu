using Newtonsoft.Json.Linq;
using System;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusExperienceTypeReader : ICensusReader<ExperienceType> {

        public override ExperienceType? ReadEntry(JToken token) {
            ExperienceType type = new ExperienceType();

            type.ID = token.GetRequiredInt32("experience_id");
            type.Name = token.GetString("description", "");
            type.Amount = (double) token.GetDecimal("xp", 0);

            return type;
        }

    }
}
