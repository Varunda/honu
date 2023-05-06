using System;
using System.Collections.Generic;
using System.Linq;
using watchtower.Models.Events;

namespace watchtower.Code {

    public class RecentSundererDestroyExpStore {

        private static RecentSundererDestroyExpStore _Instance = new RecentSundererDestroyExpStore();
        public static RecentSundererDestroyExpStore Get() { return _Instance; }

        private List<ExpEvent> _Recent = new List<ExpEvent>();

        public void Add(ExpEvent ev) {
            lock (_Recent) {
                _Recent.Add(ev);
            }
        }

        public List<ExpEvent> GetList() {
            List<ExpEvent> recent;
            lock (_Recent) {
                recent = new List<ExpEvent>(_Recent);
            }
            return recent;
        }

        /// <summary>
        ///     Cleanup all events stored in this store that occured before a timestamp
        /// </summary>
        /// <param name="when">Only events after this time are included</param>
        /// <returns>How many events were removed from this store</returns>
        public int Clean(DateTime when) {
            int count = 0;
            lock (_Recent) {
                int before = _Recent.Count;
                _Recent = _Recent.Where(iter => iter.Timestamp >= when).ToList();
                int after = _Recent.Count;
                count = before - after;
            }
            return count;
        }

    }
}
