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

    public class DirectiveTierCollection : BaseStaticCollection<DirectiveTier> {

        public DirectiveTierCollection(ILogger<DirectiveTierCollection> logger, ILoggerFactory fac,
            ICensusQueryFactory census, ICensusReader<DirectiveTier> reader)
            : base (logger, "directive_tier", census, reader) {

            /*
            _PatchFile = "./census-patches/directive_tier.json";
            _KeyFunc = (entry) => $"{entry.TreeID}:{entry.TierID}";
            _CopyFunc = (oldEntry, newEntry) => {
                newEntry.RewardSetID = oldEntry.RewardSetID;
            };
            */
        }

    }
}
