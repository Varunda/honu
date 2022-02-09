using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Code.Hubs {

    public interface IRealtimeMapHub {

        Task UpdateMap(PsZone zone);

    }
}
