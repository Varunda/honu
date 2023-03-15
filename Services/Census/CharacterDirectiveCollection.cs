using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterDirectiveCollection {

        private readonly ILogger<CharacterDirectiveCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<CharacterDirective> _Reader;

        public CharacterDirectiveCollection(ILogger<CharacterDirectiveCollection> logger,
            ICensusQueryFactory census, ICensusReader<CharacterDirective> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<CharacterDirective>> GetByCharacterID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character directive - get by character id");
            trace?.AddTag("characterID", charID);

            CensusQuery query = _Census.Create("characters_directive");
            query.SetLimit(10_000);
            query.Where("character_id").Equals(charID);

            List<CharacterDirective> dirs = await _Reader.ReadList(query);

            if (dirs.Count >= query.Limit) {
                _Logger.LogError($"Found {dirs.Count} directives when the limit was {query.Limit}, may need to page");
            }

            return dirs;
        }

    }
}
