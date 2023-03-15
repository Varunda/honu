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

    public class CharacterDirectiveTreeCollection {

        private readonly ILogger<CharacterDirectiveTreeCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<CharacterDirectiveTree> _Reader;

        public CharacterDirectiveTreeCollection(ILogger<CharacterDirectiveTreeCollection> logger,
            ICensusQueryFactory census, ICensusReader<CharacterDirectiveTree> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public async Task<List<CharacterDirectiveTree>> GetByCharacterID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character directive tree - get by character id");
            trace?.AddTag("characterID", charID);

            CensusQuery query = _Census.Create("characters_directive_tree");
            query.SetLimit(10_000);
            query.Where("character_id").Equals(charID);

            List<CharacterDirectiveTree> dirs = await _Reader.ReadList(query);

            return dirs;
        }

    }
}
