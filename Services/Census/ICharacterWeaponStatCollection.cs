using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Census {

    public interface ICharacterWeaponStatCollection {

        /// <summary>
        ///     Get all the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A list from census of all the <see cref="WeaponStatEntry"/>s that exist
        /// </returns>
        Task<List<WeaponStatEntry>> GetByCharacterID(string charID);

    }
}
