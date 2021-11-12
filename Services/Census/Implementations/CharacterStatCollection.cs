using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterStatCollection : ICharacterStatCollection {

        private readonly ILogger<CharacterStatCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<PsCharacterStat> _Reader;

        public CharacterStatCollection(ILogger<CharacterStatCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsCharacterStat> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader;
        }

        public async Task<List<PsCharacterStat>> GetByID(string charID) {
            CensusQuery query = _Census.Create("characters_stat");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            return await _Reader.ReadList(query);
        }

    }
}
