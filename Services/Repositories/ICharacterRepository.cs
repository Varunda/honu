using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        Task<List<PsCharacter>> GetByIDs(List<string> IDs, bool fast = false);

        /// <summary>
        ///     Get a <see cref="PsCharacter"/> by name
        /// </summary>
        /// <param name="name">Name of the <see cref="PsCharacter"/> to get</param>
        /// <returns>
        ///     The <see cref="PsCharacter"/> with <see cref="PsCharacter.Name"/> of <paramref name="name"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        Task<List<PsCharacter>> GetByName(string name);

        /// <summary>
        ///     Update/Insert a character
        /// </summary>
        /// <param name="character"></param>
        Task Upsert(PsCharacter character);

        /// <summary>
        ///     Search for a character by wildcard
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<List<PsCharacter>> SearchByName(string name);

    }

    public static class ICharacterRepositoryExtensions {

        public static async Task<PsCharacter?> GetFirstByName(this ICharacterRepository repo, string name) {
            List<PsCharacter> chars = await repo.GetByName(name);

            if (chars.Count == 0) {
                return null;
            }

            return chars[0];
        }

    }

}
