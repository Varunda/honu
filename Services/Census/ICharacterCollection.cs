using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface ICharacterCollection {

        /// <summary>
        ///     Get a <see cref="Ps2Character"/> by ID
        /// </summary>
        /// <param name="ID">ID of the character to get</param>
        /// <returns>
        ///     The <see cref="Ps2Character"/> with <see cref="Ps2Character.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByID(string ID);

        /// <summary>
        ///     Get a <see cref="Ps2Character"/> by name
        /// </summary>
        /// <param name="name">Name of the <see cref="Ps2Character"/> to get</param>
        /// <returns>
        ///     The <see cref="Ps2Character"/> with <see cref="Ps2Character.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByName(string name);

    }
}
