using System;
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

        public RealtimeAlertTeam VS { get; }

        public RealtimeAlertTeam NC { get; }

        public RealtimeAlertTeam TR { get; }

        public RealtimeAlert() {
            VS = new RealtimeAlertTeam();
            VS.TeamID = Faction.VS;

            NC = new RealtimeAlertTeam();
            NC.TeamID = Faction.NC;

            TR = new RealtimeAlertTeam();
            TR.TeamID = Faction.TR;
        }

        public RealtimeAlert(RealtimeAlertTeam vs, RealtimeAlertTeam nc, RealtimeAlertTeam tr) {
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

            return mini;
        }

    }

}
