using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface ICharacterRepository {

        /// <summary>
        ///     Get a <see cref="PsCharacter"/>
        /// </summary>
        /// <param name="charID">ID of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByID(string charID);

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by name
        /// </summary>
        /// <param name="name">Name of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByName(string name);

        Task Upsert(PsCharacter character);

    }
}
