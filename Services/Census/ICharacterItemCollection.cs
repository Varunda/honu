using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface ICharacterItemCollection {

        /// <summary>
        ///     Get the <see cref="CharacterItem"/> for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns></returns>
        Task<List<CharacterItem>> GetByID(string charID);

    }
}
