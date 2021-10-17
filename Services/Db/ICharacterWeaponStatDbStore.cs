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
        ///     Update or insert an entry
        /// </summary>
        /// <param name="entry">Entry to upsert</param>
        /// <returns>A task when the operation is complete</returns>
        Task Upsert(WeaponStatEntry entry);

    }
}
