using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusObjectiveSetReader : ICensusReader<ObjectiveSet> {

        public override ObjectiveSet? ReadEntry(JsonElement token) {
            ObjectiveSet type = new ObjectiveSet();

            type.ID = token.GetRequiredInt32("objective_set_id");
            type.GroupID = token.GetRequiredInt32("objective_group_id");

            return type;
        }

    }
}
