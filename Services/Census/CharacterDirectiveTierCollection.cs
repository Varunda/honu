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

    public class CharacterDirectiveTierCollection {

        private readonly ILogger<CharacterDirectiveTierCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<CharacterDirectiveTier> _Reader;

        public CharacterDirectiveTierCollection(ILogger<CharacterDirectiveTierCollection> logger,
            ICensusQueryFactory census, ICensusReader<CharacterDirectiveTier> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<CharacterDirectiveTier>> GetByCharacterID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character directive tier - get by character id");
            trace?.AddTag("honu.characterID", charID);

            CensusQuery query = _Census.Create("characters_directive_tier");
            query.SetLimit(10_000);
            query.Where("character_id").Equals(charID);

            List<CharacterDirectiveTier> dirs = await _Reader.ReadList(query);

            return dirs;
        }

    }
}
