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

        /// <summary>
        ///     Send the full alert, which has all of the events
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        Task SendFull(RealtimeAlert alert);

        Task SendAll(List<RealtimeAlert> alerts);

    }
}
