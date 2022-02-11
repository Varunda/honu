using System.Collections.Generic;
using System.Linq;
using watchtower.Models.Census;

namespace watchtower.Code {

    /// <summary>
    ///     Store of alerts currently happening
    /// </summary>
    public class AlertStore {

        private static AlertStore _Instance = new AlertStore();

        /// <summary>
        ///     Get the global singleton instance
        /// </summary>
        public static AlertStore Get() { return AlertStore._Instance; }

        private List<PsAlert> _Alerts = new List<PsAlert>();

        /// <summary>
        ///     Add a new alert
        /// </summary>
        /// <param name="alert">Alert to be stored here</param>
        public void AddAlert(PsAlert alert) {
            lock (_Alerts) {
                _Alerts.Add(alert);
            }
        }

        /// <summary>
        ///     Get a copy of the list of alerts, this is a deep copy alert
        /// </summary>
        public List<PsAlert> GetAlerts() {
            lock (_Alerts) {
                return new List<PsAlert>(_Alerts);
            }
        }

        /// <summary>
        ///     Remove an alert by its ID
        /// </summary>
        /// <param name="ID">ID of the alert to remove</param>
        public void RemoveByID(long ID) {
            lock (_Alerts) {
                _Alerts = _Alerts.Where(iter => iter.ID != ID).ToList();
            }
        }

    }
}
