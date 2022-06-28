using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Health;
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
        /// <param name="chars">List of characters to be included in a report</param>
        Task UpdateCharacters(List<PsCharacter> chars);

        /// <summary>
        ///     Sent when all kill have been populated
        /// </summary>
        /// <param name="events">List of kill events to include in the report</param>
        Task UpdateKills(List<KillEvent> events);

        /// <summary>
        ///     Send when the kills have been populated
        /// </summary>
        /// <param name="events">List of death events to be included in the report</param>
        Task UpdateDeaths(List<KillEvent> events);

        /// <summary>
        ///     Sent when exp events have been populated
        /// </summary>
        /// <param name="events">List of exp events to be included in the report</param>
        Task UpdateExp(List<ExpEvent> events);

        /// <summary>
        ///     Sent when the items have been populated
        /// </summary>
        /// <param name="items">List of items to be included in the report</param>
        Task UpdateItems(List<PsItem> items);

        Task UpdateItemCategories(List<ItemCategory> cats);

        /// <summary>
        ///     Sent when outfits have been populated
        /// </summary>
        /// <param name="outfits">Outfits to be included in the report</param>
        Task UpdateOutfits(List<PsOutfit> outfits);

        /// <summary>
        ///     Sent when sessions have been populated
        /// </summary>
        /// <param name="sessions">Sessions to be included in the report</param>
        Task UpdateSessions(List<Session> sessions);

        Task UpdateControls(List<FacilityControlEvent> events);

        Task UpdatePlayerControls(List<PlayerControlEvent> events);

        Task UpdateFacilities(List<PsFacility> facilities);

        Task UpdateReconnects(List<RealtimeReconnectEntry> entries);

        /// <summary>
        ///     Sent when an error occurs while generating the report. It is expected the client closes the connection after
        /// </summary>
        /// <param name="err">Error to be sent</param>
        Task SendError(string err);

    }
}
