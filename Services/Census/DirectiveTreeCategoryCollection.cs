using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class DirectiveTreeCategoryCollection : BaseStaticCollection<DirectiveTreeCategory> {

        public DirectiveTreeCategoryCollection(ILogger<DirectiveTreeCategoryCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTreeCategory> reader)
            : base(logger, "directive_tree_category", census, reader) {

            _PatchFile = "./census-patches/directive_tree_category.json";
            _KeyFunc = (entry) => $"{entry.ID}";
        }

    }
}
