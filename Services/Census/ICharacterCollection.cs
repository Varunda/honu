using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface ICharacterCollection {

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by ID
        /// </summary>
        /// <param name="ID">ID of the character to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByID(string ID);

        /// <summary>
        ///     Get a list of characters by IDs
        /// </summary>
        /// <param name="IDs">IDs to get from Census</param>
        /// <returns>
        ///     A list of <see cref="PsCharacter"/> with a <see cref="PsCharacter.ID"/>
        ///     as an element of <paramref name="IDs"/>
        /// </returns>
        Task<List<PsCharacter>> GetByIDs(List<string> IDs);

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by name
        /// </summary>
        /// <param name="name">Name of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<PsCharacter?> GetByName(string name);

        /// <summary>
        ///     Search for characters by name
        /// </summary>
        /// <remarks>
        ///     Census does not support wildcard searching with less than 3 characters, and if
        ///     less than 3 characters are passed, an empty list will be returned
        /// </remarks>
        /// <param name="name">Name to search by</param>
        /// <param name="stop">Stopping token</param>
        /// <returns>
        ///     A list of <see cref="PsCharacter"/>s that match the name given
        /// </returns>
        Task<List<PsCharacter>> SearchByName(string name, CancellationToken stop);

    }

    public static class ICharacterCollectionExtensionMethods {

        /// <summary>
        ///     Search for characters by name
        /// </summary>
        /// <remarks>
        ///     Census does not support wildcard searching with less than 3 characters, and if
        ///     less than 3 characters are passed, an empty list will be returned
        /// </remarks>
        /// <param name="census">Extension instance</param>
        /// <param name="name">Name to search for</param>
        /// <returns>
        ///     A list of <see cref="PsCharacter"/>s that partially match the name given
        /// </returns>
        public static Task<List<PsCharacter>> SearchByName(this ICharacterCollection census, string name) => census.SearchByName(name, CancellationToken.None);

    }

}
