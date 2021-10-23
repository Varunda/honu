using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public interface ICharacterHistoryStatCollection {

        /// <summary>
        ///     Get the entries from census
        /// </summary>
        /// <param name="charID">Character ID to get</param>
        /// <returns></returns>
        Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID);

    }
}
