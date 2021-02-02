using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Census {

    /// <summary>
    /// Manages <see cref="Character"/>s
    /// </summary>
    public interface ICharacterCollection {

        /// <summary>
        /// Get a character by it's ID
        /// </summary>
        /// <param name="ID">ID of the character to get</param>
        /// <returns>The <see cref="Character"/> with <see cref="Character.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it doesn't exist</returns>
        Task<Character?> GetByIDAsync(string ID);

        Task<Character?> GetByNameAsync(string name);

        /// <summary>
        /// Cache a new character, not wanting a return
        /// </summary>
        /// <param name="ID">ID of the character to cache</param>
        void Cache(string ID);

        /// <summary>
        /// Get all cached <see cref="Character"/>s
        /// </summary>
        /// <returns></returns>
        List<Character> GetCache();

        Task CacheBlock(List<string> IDs);

    }
}
