using watchtower.Constants;

namespace watchtower.Models.Watchtower {

    public class RealtimeClassUsage {

        public short FactionID { get; set; } = Faction.UNKNOWN;

        public int Total { get; set; }

        public int Infil { get; set; }

        public int LightAssault { get; set; }

        public int CombatMedic { get; set; }

        public int Engineer { get; set; }

        public int HeavyAssault { get; set; }

        public int MAX { get; set; }

        public int Unknown { get; set; }

    }
}
