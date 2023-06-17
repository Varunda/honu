using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusDirectiveTreeReader : ICensusReader<DirectiveTree> {

        public override DirectiveTree? ReadEntry(JsonElement token) {
            DirectiveTree tree = new DirectiveTree();

            tree.ID = token.GetRequiredInt32("directive_tree_id");
            tree.Name = token.GetChild("name")?.GetString("en", "<missing name>") ?? "<missing name>";
            tree.CategoryID = token.GetInt32("directive_tree_category_id", 0);
            tree.ImageSetID = token.GetInt32("image_set_id", 0);
            tree.ImageID = token.GetInt32("image_id", 0);

            return tree;
        }

    }
}
