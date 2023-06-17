using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusDirectiveTreeCategoryReader : ICensusReader<DirectiveTreeCategory> {

        public override DirectiveTreeCategory? ReadEntry(JsonElement token) {
            DirectiveTreeCategory cat = new DirectiveTreeCategory();

            cat.ID = token.GetRequiredInt32("directive_tree_category_id");
            cat.Name = token.GetChild("name")?.GetString("en", "<missing name>") ?? "<missing name>";

            return cat;
        }

    }
}
