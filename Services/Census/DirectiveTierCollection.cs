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

    public class DirectiveTierCollection {

        private readonly ILogger<DirectiveTierCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<DirectiveTier> _Reader;

        public DirectiveTierCollection(ILogger<DirectiveTierCollection> logger,
            ICensusQueryFactory census, ICensusReader<DirectiveTier> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<DirectiveTier>> GetAll() {
            CensusQuery query = _Census.Create("directive_tier");
            query.SetLimit(10_000);

            List<DirectiveTier> dirs = await _Reader.ReadList(query);
            return dirs;
        }

    }
}
