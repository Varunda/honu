using System;
using System.Collections.Generic;
using watchtower.Constants;
using watchtower.Models.Census;

namespace watchtower.Models.RealtimeAlert {

    /// <summary>
    ///     Information about an outfit wars match
    /// </summary>
    public class RealtimeAlert {

        /// <summary>
        ///     When this match started
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     ID of the world this match is taking place on
        /// </summary>
        public short WorldID { get; set; }

        /// <summary>
        ///     Zone ID of where this match is taking place
        /// </summary>
        public uint ZoneID { get; set; }

        public PsZone Zone { get; set; } = new();

        public List<PsFacilityOwner> Facilities { get; set; } = new();

        /// <summary>
        ///     Represents a summary of what has happened with characters with a team_id of VS
        /// </summary>
        public RealtimeAlertTeam VS { get; }

        /// <summary>
        ///     Represents a summary of what has happened with characters with a team_id of NC
        /// </summary>
        public RealtimeAlertTeam NC { get; }

        /// <summary>
        ///     Represents a summary of what has happened with characters with a team_id of TR
        /// </summary>
        public RealtimeAlertTeam TR { get; }

        public RealtimeAlert() {
            VS = new RealtimeAlertTeam();
            VS.TeamID = Faction.VS;

            NC = new RealtimeAlertTeam();
            NC.TeamID = Faction.NC;

            TR = new RealtimeAlertTeam();
            TR.TeamID = Faction.TR;
        }

        private RealtimeAlert(RealtimeAlertTeam vs, RealtimeAlertTeam nc, RealtimeAlertTeam tr) {
            VS = new RealtimeAlertTeam(vs);
            NC = new RealtimeAlertTeam(nc);
            TR = new RealtimeAlertTeam(tr);
        }

        /// <summary>
        ///     Turn the alert into a mini alert, which has the same information, but without the events
        /// </summary>
        /// <returns></returns>
        public RealtimeAlert AsMini() {
            RealtimeAlert mini = new(VS, NC, TR);
            mini.WorldID = WorldID;
            mini.ZoneID = ZoneID;
            mini.Timestamp = Timestamp;
            mini.Facilities = Facilities;

            return mini;
        }

    }

}
