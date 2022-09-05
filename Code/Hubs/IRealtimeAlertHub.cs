using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models.RealtimeAlert;

namespace watchtower.Code.Hubs {

    public interface IRealtimeAlertHub {

        /// <summary>
        ///     Send a small update, which does not include the full list of events in the alert
        /// </summary>
        /// <param name="alert">Alert to be sent</param>
        Task UpdateAlert(RealtimeAlert alert);

        Task RemoteCall(string action);

    }
}
