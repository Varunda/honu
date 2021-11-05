using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.CharacterViewer.WeaponStats;

namespace watchtower.Services.Db {

    public interface ICharacterWeaponStatDbStore {

        /// <summary>
        ///     Get the <see cref="WeaponStatEntry"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     A list of all the <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.CharacterID"/>
        ///     of <paramref name="charID"/>
        /// </returns>
        Task<List<WeaponStatEntry>> GetByCharacterID(string charID);

        /// <summary>
        ///     Get all the <see cref="WeaponStatEntry"/> for a weapon
        /// </summary>
        /// <param name="itemID">ID of the weapon</param>
        /// <param name="minKills">Minimum number of kills to be included</param>
        /// <returns>
        ///     A list of all <see cref="WeaponStatEntry"/> with <see cref="WeaponStatEntry.WeaponID"/> of <paramref name="itemID"/>
        /// </returns>
        Task<List<WeaponStatEntry>> GetByItemID(string itemID, int? minKills = null);

        /// <summary>
        ///     Update or insert an entry
        /// </summary>
        /// <param name="entry">Entry to upsert</param>
        /// <returns>A task when the operation is complete</returns>
        Task Upsert(WeaponStatEntry entry);

    }
}
