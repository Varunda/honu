using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface ICharacterDbStore {

        /// <summary>
        ///     Get a character from the backing database by ID
        /// </summary>
        /// <param name="charID">ID of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.ID"/> of <paramref name="charID"/>,
        ///     or <c>null</c> if it doesn't exist in the database, but it may exist in Census
        /// </returns>
        Task<PsCharacter?> GetByID(string charID);

        /// <summary>
        ///     Get a block of characters
        /// </summary>
        /// <param name="IDs">List of IDs to get</param>
        /// <returns>
        ///     A list of all <see cref="PsCharacter"/>s that have an ID as an element of <paramref name="IDs"/>
        /// </returns>
        Task<List<PsCharacter>> GetByIDs(List<string> IDs);

        /// <summary>
        ///     Get a character from the backing database by name
        /// </summary>
        /// <param name="name">Name of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist in the database, but it may exist in Census
        /// </returns>
        Task<List<PsCharacter>> GetByName(string name);

        /// <summary>
        ///     Insert or update (Upsert) a character in the backing database
        /// </summary>
        /// <param name="character">Character parameters to pass</param>
        /// <returns>
        ///     A task for when the operation is complete
        /// </returns>
        Task Upsert(PsCharacter character);

    }

    public static class ICharacterDbStoreExtensionMethods {

        /// <summary>
        ///     Get the first character that has the name of <paramref name="name"/>. Case-insensitive
        /// </summary>
        /// <param name="repo">Extension instance</param>
        /// <param name="name">Name of the character to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>. If multiple exist,
        ///     the first one loaded (by ID) is returned
        /// </returns>
        public static async Task<PsCharacter?> GetFirstByName(this ICharacterDbStore repo, string name) {
            List<PsCharacter> chars = await repo.GetByName(name);

            if (chars.Count == 0) {
                return null;
            }

            return chars[0];
        }

    }

}
