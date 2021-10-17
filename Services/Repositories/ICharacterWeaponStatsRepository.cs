using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Repositories {

    public interface ICharacterWeaponStatRepository {

        Task<List<WeaponStatEntry>> GetByCharacterID(string charID);

    }

}
