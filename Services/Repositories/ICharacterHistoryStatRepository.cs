using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface ICharacterHistoryStatRepository {

        /// <summary>
        ///     Get the <see cref="PsCharacterHistoryStat"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="PsCharacterHistoryStat"/> with <see cref="PsCharacterHistoryStat.CharacterID"/> of <paramref name="charID"/>
        /// </returns>
        Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID);

        /// <summary>
        ///     Update/Insert (update) a <see cref="PsCharacterHistoryStat"/>
        /// </summary>
        /// <param name="charID">Character ID</param>
        /// <param name="type">Type</param>
        /// <param name="stat">Parameters</param>
        /// <returns>
        ///     A task when the operation is complete
        /// </returns>
        Task Upsert(string charID, string type, PsCharacterHistoryStat stat);

    }
}
