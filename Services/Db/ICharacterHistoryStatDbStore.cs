using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface ICharacterHistoryStatDbStore {

        /// <summary>
        ///     Get all <see cref="PsCharacterHistoryStat"/>s for a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     All <see cref="PsCharacterHistoryStat"/>s with <see cref="PsCharacterHistoryStat.CharacterID"/> of <paramref name="charID"/>,
        ///     or an empty list if no character exists
        /// </returns>
        Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID);

        /// <summary>
        ///     GEt all <see cref="PsCharacterHistoryStat"/>s for all characters with ID passed in <paramref name="IDs"/>
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        Task<List<PsCharacterHistoryStat>> GetByCharacterIDs(List<string> IDs);

        /// <summary>
        ///     Update/Insert (upsert) a <see cref="PsCharacterHistoryStat"/> entry
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="type">What type this stat is for</param>
        /// <param name="stat">Parameters of the stat used when upserting</param>
        /// <returns>
        ///     A task for when the operation is complete 
        /// </returns>
        Task Upsert(string charID, string type, PsCharacterHistoryStat stat);

    }
}
