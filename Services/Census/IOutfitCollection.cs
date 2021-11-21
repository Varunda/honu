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

        /// <summary>
        ///     Get a <see cref="PsOutfit"/> by its tag (alias)
        /// </summary>
        /// <param name="tag">Tag to get</param>
        /// <returns>
        ///     The <see cref="PsOutfit"/> with <see cref="PsOutfit.Tag"/> of <paramref name="tag"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsOutfit?> GetByTag(string tag);

        /// <summary>
        ///     Get the members of an outfit
        /// </summary>
        /// <param name="outfitID">ID of the outfit</param>
        /// <returns>
        ///     A list of <see cref="OutfitMember"/>, representing the members of an outfit. If the outfit does
        ///     not exist, an empty list is returned
        /// </returns>
        Task<List<OutfitMember>> GetMembers(string outfitID);

    }
}
