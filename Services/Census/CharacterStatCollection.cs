using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterStatCollection {

        private readonly ILogger<CharacterStatCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<PsCharacterStat> _Reader;

        public CharacterStatCollection(ILogger<CharacterStatCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsCharacterStat> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader;
        }

        /// <summary>
        ///     Get the <see cref="PsCharacterStat"/>s of a character
        /// </summary>
        /// <param name="charID">Character ID</param>
        public async Task<List<PsCharacterStat>> GetByID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character stats - get by character id");
            trace?.AddTag("characterID", charID);

            CensusQuery query = _Census.Create("characters_stat");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            return await _Reader.ReadList(query);
        }

    }
}
