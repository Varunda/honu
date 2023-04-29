using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Wrapped;

namespace watchtower.Code.Hubs {

    public interface IWrappedHub {

        Task UpdateStatus(string status);

        Task SendMessage(string msg);

        Task SendWarning(string msg);

        Task SendError(string msg);

        Task SendWrappedEntry(WrappedEntry entry);

        Task UpdateInputCharacters(List<PsCharacter> inputCharacters);

        Task MarkCharacterLoaded(string charID);

        Task SendSessions(List<Session> sessions);

        Task SendKills(List<KillEvent> events);

        Task SendDeaths(List<KillEvent> events);

        Task SendExp(List<ExpEvent> events);

        Task SendVehicleDestroy(List<VehicleDestroyEvent> events);

        Task SendFacilityControl(List<FacilityControlEvent> events);

        Task SendItemAdded(List<ItemAddedEvent> events);

        Task SendAchievementEarned(List<AchievementEarnedEvent> events);

        Task UpdateItems(List<PsItem> items);

        Task UpdateCharacters(List<PsCharacter> chars);

        Task UpdateOutfits(List<PsOutfit> outfits);

        Task UpdateFacilities(List<PsFacility> facs);

        Task UpdateExpTypes(List<ExperienceType> types);

        Task UpdateAchievements(List<Achievement> achs);

        Task UpdateFireGroupToFireMode(List<FireGroupToFireMode> xrefs);

    }

}
