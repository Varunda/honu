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

    public class DirectiveTreeCollection : BaseStaticCollection<DirectiveTree> {

        public DirectiveTreeCollection(ILogger<DirectiveTreeCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTree> reader)
            : base(logger, "directive_tree", census, reader) {

            _PatchFile = "./census-patches/directive_tree.json";
            _KeyFunc = (entry) => $"{entry.ID}";
        }

    }
}
