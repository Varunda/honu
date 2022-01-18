using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using watchtower.Models.Events;
using watchtower.Models.Watchtower;

namespace watchtower.Code {

    /// <summary>
    ///     Store that contains recent facility control events
    /// </summary>
    public class RecentFacilityControlStore {

        private static RecentFacilityControlStore _Instance = new RecentFacilityControlStore();
        public static RecentFacilityControlStore Get() { return RecentFacilityControlStore._Instance; }

        private ConcurrentDictionary<short, RecentWorldFacilityControlEvents> _RecentEvents = new();

        public void Add(short worldID, FacilityControlEvent ev) {
            lock (_RecentEvents) {
                RecentWorldFacilityControlEvents recent = _RecentEvents.GetOrAdd(worldID, new RecentWorldFacilityControlEvents());

                if (ev.NewFactionID == ev.OldFactionID) {
                    recent.Defenses.Add(ev);

                    if (recent.Defenses.Count > 10) {
                        recent.Defenses.RemoveAt(10);
                    }
                } else {
                    recent.Captures.Add(ev);

                    if (recent.Captures.Count > 10) {
                        recent.Captures.RemoveAt(10);
                    }
                }

            }
        }

        public RecentWorldFacilityControlEvents Get(short worldID) {
            RecentWorldFacilityControlEvents recent;
            lock (_RecentEvents) {
                recent = _RecentEvents.GetOrAdd(worldID, new RecentWorldFacilityControlEvents());
            }

            return new RecentWorldFacilityControlEvents(recent);
        }

    }

}
