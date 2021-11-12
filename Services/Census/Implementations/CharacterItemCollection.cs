using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterItemCollection : ICharacterItemCollection {

        private readonly ILogger<CharacterItemCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        private readonly ICensusReader<CharacterItem> _Reader;

        public CharacterItemCollection(ILogger<CharacterItemCollection> logger,
            ICensusReader<CharacterItem> reader, ICensusQueryFactory census) {

            _Logger = logger;

            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _Census = census;
        }

        public Task<List<CharacterItem>> GetByID(string charID) {
            CensusQuery query = _Census.Create("characters_item");
            query.Where("character_id").Equals(charID);

            return _Reader.ReadList(query);
        }

    }
}
