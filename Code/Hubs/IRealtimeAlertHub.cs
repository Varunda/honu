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
        ///     Tell a client to call a function
        /// </summary>
        /// <param name="action">Function name to call</param>
        Task RemoteCall(string action);

        /// <summary>
        ///     Tell a client that a facility was captured
        /// </summary>
        /// <param name="facilityID">ID of the facility</param>
        /// <param name="teamID">ID of the team/faction that captured the base</param>
        Task FacilityCapture(int facilityID, short teamID);

    }
}
