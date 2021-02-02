using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class TrackedPlayer {

        public string ID { get; set; } = "";

        public string FactionID { get; set; } = "";

        public List<int> Kills { get; set; } = new List<int>();

        public List<int> Deaths { get; set; } = new List<int>();

        public List<int> Heals { get; set; } = new List<int>();

        public List<int> Revives { get; set; } = new List<int>();

        public List<int> Resupplies { get; set; } = new List<int>();

        public List<int> Repairs { get; set; } = new List<int>();

        public List<int> SundySpawns { get; set; } = new List<int>();

    }
}
