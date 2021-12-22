using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusDirectiveReader : ICensusReader<PsDirective> {

        public override PsDirective? ReadEntry(JToken token) {
            PsDirective dir = new PsDirective();

            dir.ID = token.GetRequiredInt32("directive_id");
            dir.TreeID = token.GetInt32("directive_tree_id", 0);
            dir.TierID = token.GetInt32("directive_tier_id", 0);
            dir.ObjectiveSetID = token.GetInt32("objective_set_id", 0);
            dir.Name = token.SelectToken("name")?.GetString("en", "<missing name>") ?? "<missing name>";
            dir.Description = token.SelectToken("description")?.GetString("en", "<missing description>") ?? "<missing description>";
            dir.ImageSetID = token.GetInt32("image_set_id", 0);
            dir.ImageID = token.GetInt32("image_id", 0);

            return dir;
        }

    }
}
