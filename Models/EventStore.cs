using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Events;

namespace watchtower.Models {

    public class EventStore {

        private static EventStore _Instance = new EventStore();

        public static EventStore Get() { return EventStore._Instance; }

        public List<PsEvent> TrKills { get; set; } = new List<PsEvent>();

        public List<PsEvent> TrHeals { get; set; } = new List<PsEvent>();

        public List<PsEvent> TrRevives { get; set; } = new List<PsEvent>();

        public List<PsEvent> TrResupplies { get; set; } = new List<PsEvent>();

        public List<PsEvent> TrRepairs { get; set; } = new List<PsEvent>();

        public List<PsEvent> NcKills { get; set; } = new List<PsEvent>();

        public List<PsEvent> NcHeals { get; set; } = new List<PsEvent>();

        public List<PsEvent> NcRevives { get; set; } = new List<PsEvent>();

        public List<PsEvent> NcResupplies { get; set; } = new List<PsEvent>();

        public List<PsEvent> NcRepairs { get; set; } = new List<PsEvent>();

        public List<PsEvent> VsKills { get; set; } = new List<PsEvent>();

        public List<PsEvent> VsHeals { get; set; } = new List<PsEvent>();

        public List<PsEvent> VsRevives { get; set; } = new List<PsEvent>();

        public List<PsEvent> VsResupplies { get; set; } = new List<PsEvent>();

        public List<PsEvent> VsRepairs { get; set; } = new List<PsEvent>();

    }
}
