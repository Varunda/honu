using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;

namespace watchtower.Models {

    public class PlayerCount {
        public int All { get; set; }
        public int VS { get; set; }
        public int NC { get; set; }
        public int TR { get; set; }
        public int Unknown { get; set; }
    }

    public class TerritoryControl {
        public int VS { get; set; }
        public int NC { get; set; }
        public int TR { get; set; }
        public int Total { get; set; }
    }


    /// <summary>
    /// Represents state data of a zone
    /// </summary>
    public class ZoneState {

        public ZoneState() {

        }

        public ZoneState(ZoneState other) {
            ZoneID = other.ZoneID;
            WorldID = other.WorldID;
            IsOpened = other.IsOpened;
            UnstableState = other.UnstableState;
            AlertStart = other.AlertStart;
            AlertEnd = other.AlertEnd;
            PlayerCount = other.PlayerCount;
            Players = other.Players;
            TerritoryControl = other.TerritoryControl;
        }

        /// <summary>
        /// ID of the zone
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        /// What world this zone datat is for
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        /// Is this zone currently open
        /// </summary>
        public bool IsOpened { get; set; } = false;

        /// <summary>
        ///     Used to tell if a zone is in single/double lane modes or not
        /// </summary>
        public UnstableState UnstableState { get; set; }

        /// <summary>
        /// When did an alert on this zone start?
        /// </summary>
        public DateTime? AlertStart { get; set; }

        /// <summary>
        /// When will the alert on this zone end, useful for like deso
        /// </summary>
        public DateTime? AlertEnd { get; set; }

        public int PlayerCount { get; set; }

        public PlayerCount Players { get; set; } = new PlayerCount();

        public TerritoryControl TerritoryControl { get; set; } = new TerritoryControl();

    }
}
