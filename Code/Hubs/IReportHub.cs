using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Report;

namespace watchtower.Code.Hubs {

    public interface IReportHub {

        /// <summary>
        ///     Initial send of the report with no data in it
        /// </summary>
        Task SendReport(OutfitReport report);

        /// <summary>
        ///     Sent when all character IDs have been populated
        /// </summary>
        Task UpdateCharacterIDs(OutfitReport report);

        /// <summary>
        ///     Sent when all characters have been populated
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        Task UpdateCharacters(OutfitReport report);

        /// <summary>
        ///     Sent when all kill/deaths have been populated
        /// </summary>
        Task UpdateKillDeaths(OutfitReport report);

    }
}
