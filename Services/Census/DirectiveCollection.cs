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

    public class DirectiveCollection {

        private readonly ILogger<DirectiveCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<PsDirective> _Reader;

        public DirectiveCollection(ILogger<DirectiveCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsDirective> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<PsDirective>> GetAll() {
            CensusQuery query = _Census.Create("directive");
            query.SetLimit(10_000);

            List<PsDirective> dirs = await _Reader.ReadList(query);

            if (dirs.Count >= query.Limit) {
                _Logger.LogError($"Found {dirs.Count} directives when the limit was {query.Limit}, may need to page");
            }

            return dirs;
        }

    }
}
