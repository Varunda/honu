using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
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
        Task UpdateCharacterIDs(List<string> IDs);

        /// <summary>
        ///     Sent when all characters have been populated
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        Task UpdateCharacters(List<PsCharacter> chars);

        /// <summary>
        ///     Sent when all kill/deaths have been populated
        /// </summary>
        Task UpdateKills(List<KillEvent> events);

        Task UpdateDeaths(List<KillEvent> events);

        Task UpdateExp(List<ExpEvent> events);

        Task UpdateItems(List<PsItem> items);

        Task UpdateOutfits(List<PsOutfit> outfits);

        Task UpdateSessions(List<Session> sessions);

    }
}
