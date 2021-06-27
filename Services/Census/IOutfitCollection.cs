using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface IOutfitCollection {

        /// <summary>
        ///     Get a <see cref="PsOutfit"/> by its ID
        /// </summary>
        /// <param name="outfitID">ID of the outfit to get</param>
        /// <returns>
        ///     The <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsOutfit?> GetByID(string outfitID);

    }
}
