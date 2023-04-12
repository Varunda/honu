using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Code.Hubs {

    public interface IWrappedHub {

        Task UpdateStatus(string status);

        Task SendMessage(string msg);

        Task SendWarning(string msg);

        Task SendError(string msg);

        Task UpdateInputCharacters(List<PsCharacter> inputCharacters);

    }
}
