using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Repositories {

    public interface ICharacterWeaponStatRepository {

        /// <summary>
        ///     Get the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        Task<List<WeaponStatEntry>> GetByCharacterID(string charID);

    }

}
