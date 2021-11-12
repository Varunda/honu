using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface ICharacterItemRepository {

        Task<List<CharacterItem>> GetByID(string charID);

    }
}
