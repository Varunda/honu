using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Alert;
using watchtower.Models.Census;

namespace watchtower.Code.Hubs {

    public interface IAlertHub {

        Task SendMessage(string msg);

        Task SendError(string err);

        Task SendAlert(PsAlert alert);

        Task SendCharacters(List<MinimalCharacter> chars);

        Task SendOutfits(List<PsOutfit> outfits);

        Task SendPlayerData(List<AlertPlayerDataEntry> data);

    }
}
