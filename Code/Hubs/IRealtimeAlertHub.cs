using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.RealtimeAlert;

namespace watchtower.Code.Hubs {

    public interface IRealtimeAlertHub {

        Task UpdateAlert(RealtimeAlert alert);

        Task SendFull(RealtimeAlert alert);

        Task SendAll(List<RealtimeAlert> alerts);

    }
}
