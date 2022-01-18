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

        public void Clean(int secondsBack) {
            lock (_Recent) {
                _Recent = _Recent.Where(iter => DateTime.UtcNow - iter.Timestamp <= TimeSpan.FromSeconds(secondsBack)).ToList();
            }
        }

    }
}
