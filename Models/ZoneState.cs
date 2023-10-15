using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Census;

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
    ///     Represents state data of a zone
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
            Alert = other.Alert;
            AlertInfo = other.AlertInfo;
            LastLocked = other.LastLocked;
        }

        /// <summary>
        ///     ID of the zone
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        ///     What world this zone data is for
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Is this zone currently open
        /// </summary>
        public bool IsOpened { get; set; } = false;

        /// <summary>
        ///     Used to tell if a zone is in single/double lane modes or not
        /// </summary>
        public UnstableState UnstableState { get; set; }

        /// <summary>
        ///     Alert currently running in this zone
        /// </summary>
        public PsAlert? Alert { get; set; }

        /// <summary>
        ///     Information about the alert
        /// </summary>
        public PsMetagameEvent? AlertInfo { get; set; }

        /// <summary>
        ///     When did an alert on this zone start?
        /// </summary>
        public DateTime? AlertStart { get; set; }

        /// <summary>
        ///     When will the alert on this zone end, useful for like deso
        /// </summary>
        public DateTime? AlertEnd { get; set; }

        /// <summary>
        ///     When this continent was last locked. Based on the ContinentLock event, and persisted in a DB
        /// </summary>
        public DateTime? LastLocked { get; set; }

        /// <summary>
        ///     How many players are on this zone
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        ///     Breakdown of player counts
        /// </summary>
        public PlayerCount Players { get; set; } = new PlayerCount();

        /// <summary>
        ///     Breakdown of what faction owns what territories
        /// </summary>
        public TerritoryControl TerritoryControl { get; set; } = new TerritoryControl();

    }
}
