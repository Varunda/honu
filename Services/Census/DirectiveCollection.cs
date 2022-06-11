using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class DirectiveCollection : BaseStaticCollection<PsDirective> {

        public DirectiveCollection(ILogger<DirectiveCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsDirective> reader)
            : base(logger, "directive", census, reader) {

            _PatchFile = "./census-patches/directive.json";
            _KeyFunc = (entry) => $"{entry.ID}";
            _CopyFunc = (oldEntry, newEntry) => {
                newEntry.ObjectiveSetID = oldEntry.ObjectiveSetID;
            };
        }

    }
}
