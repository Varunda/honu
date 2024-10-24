namespace watchtower.Code.Constants {

    public class MetagameEventType {

        /// <summary>
        ///     old versions of the capture continent alerts. these are per-zone,
        ///     while <see cref="CAPTURE_ZONE_MELTDOWN"/> and <see cref="CAPTURE_ZONE_FULL"/> is per-faction AND per-zone 
        /// </summary>
        public const int CAPTURE_ZONE_OLD = 1;

        /// <summary>
        ///     used for capturing specific types of facilities, like biolabs, tech plants, etc.
        /// </summary>
        public const int CAPTURE_FACILITY = 2;

        /// <summary>
        ///     outfit wars pre-match, forgotten fleet carrier (ghost bastion), nanite storm (?)
        /// </summary>
        public const int OTHER = 4;

        /// <summary>
        ///     ???
        /// </summary>
        public const int STABILIZE = 5;

        /// <summary>
        ///     maximum pressure, sudden death
        /// </summary>
        public const int KILLS = 6;

        /// <summary>
        ///     these _seem_ to be the 45 minute lockdowns, while <see cref="CAPTURE_ZONE_FULL"/> is the 90 minutes
        /// </summary>
        public const int CAPTURE_ZONE_MELTDOWN = 8;

        /// <summary>
        ///     alert used for lockdown when the full continent is opened
        /// </summary>
        public const int CAPTURE_ZONE_FULL = 9;

        /// <summary>
        ///     aerial anomaly, cortium ("Refine and Refuel"), desolation outfit wars
        /// </summary>
        public const int GATHER_RESOURCE = 10;

    }
}
