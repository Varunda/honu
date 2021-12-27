using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusObjectiveTypeReader : ICensusReader<ObjectiveType> {

        public override ObjectiveType? ReadEntry(JToken token) {
            ObjectiveType type = new ObjectiveType();

            type.ID = token.GetRequiredInt32("objective_type_id");
            type.Description = token.GetString("description", "<missing description>");
            type.Param1 = token.Value<string?>("param1");
            type.Param2 = token.Value<string?>("param2");
            type.Param3 = token.Value<string?>("param3");
            type.Param4 = token.Value<string?>("param4");
            type.Param5 = token.Value<string?>("param5");
            type.Param6 = token.Value<string?>("param6");
            type.Param7 = token.Value<string?>("param7");
            type.Param8 = token.Value<string?>("param8");
            type.Param9 = token.Value<string?>("param9");
            type.Param10 = token.Value<string?>("param10");

            return type;
        }

    }
}
