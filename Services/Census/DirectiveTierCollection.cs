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

    public class DirectiveTierCollection : BaseStaticCollection<DirectiveTier> {

        private readonly ILogger<DirectiveTierCollection> _Logger;

        public DirectiveTierCollection(ILogger<DirectiveTierCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTier> reader)
            : base ("directive_tier", census, reader) {

            _Logger = logger;
        }

    }
}
