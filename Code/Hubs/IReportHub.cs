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
        ///     Update the client on what step has just been performed
        /// </summary>
        /// <param name="state">State, pull this from <see cref="OutfitReportState"/></param>
        Task UpdateState(string state);

        /// <summary>
        ///     Send the parameters used to generate the report
        /// </summary>
        Task SendParameters(OutfitReportParameters report);

        /// <summary>
        ///     Tell the client the character IDs that are tracked for the report,
        ///     and how many will be loading events. This is used for progress bars
        /// </summary>
        Task SendCharacterIDs(List<string> ids);

        /// <summary>
        ///     Send the client the kills for a specific character
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendKills(string characterID, List<KillEvent> events);

        /// <summary>
        ///     Send the client the deaths for a specific character
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendDeaths(string characterID, List<KillEvent> events);

        /// <summary>
        ///     Send the client the exp events for a specific character
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendExp(string characterID, List<ExpEvent> events);

        /// <summary>
        ///     Send the <see cref="VehicleDestroyEvent"/>s a single character performed
        /// </summary>
        /// <param name="characterID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendVehicleDestroy(string characterID, List<VehicleDestroyEvent> events);

        /// <summary>
        ///     Send the <see cref="PlayerControlEvent"/>s a single character did
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendPlayerControl(string charID, List<PlayerControlEvent> events);

        /// <summary>
        ///     Send the <see cref="AchievementEarnedEvent"/>s a single character earned
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendAchievementEarned(string charID, List<AchievementEarnedEvent> events);

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

        Task UpdateVehicleDestroy(List<VehicleDestroyEvent> events);

        /// <summary>
        ///     Sent when the items have been populated
        /// </summary>
        /// <param name="items">List of items to be included in the report</param>
        Task UpdateItems(List<PsItem> items);

        /// <summary>
        ///     Send the item categories
        /// </summary>
        Task UpdateItemCategories(List<ItemCategory> cats);

        Task UpdateExperienceTypes(List<ExperienceType> types);

        Task UpdateAchievementEarned(List<AchievementEarnedEvent> events);

        Task UpdateAchievements(List<Achievement> achs);

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

        /// <summary>
        ///     Send a message to the client
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task SendMessage(string msg);

    }
}
