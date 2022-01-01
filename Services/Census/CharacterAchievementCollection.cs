using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterAchievementCollection {

        private readonly ILogger<CharacterAchievementCollection> _Logger;
        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<CharacterAchievement> _Reader;

        public CharacterAchievementCollection(ILogger<CharacterAchievementCollection> logger,
            ICensusQueryFactory census, ICensusReader<CharacterAchievement> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader;
        }

        public async Task<List<CharacterAchievement>> GetByCharacterID(string charID) {
            CensusQuery query = _Census.Create("characters_achievement");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            List<CharacterAchievement> achs = await _Reader.ReadList(query);
            return achs;
        }

    }
}
