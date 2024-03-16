using System;
using watchtower.Models.Census;

namespace watchtower.Models.Api {

    /// <summary>
    ///     mirror the info returned from PS2Alerts
    /// </summary>
    public class PS2AlertInfo {

        /// <summary>
        ///     ID of the <see cref="PsAlert"/> that Honu tracks
        /// </summary>
        public long HonuId { get; set; }

        /// <summary>
        ///     ID of the world
        /// </summary>
        public short World { get; set; }

        /// <summary>
        ///     the instance ID of the alert (an incrementing ID)
        /// </summary>
        public int CensusInstanceId { get; set; }

        /// <summary>
        ///     same thing as above?
        /// </summary>
        public string InstanceId { get; set; } = "";

        /// <summary>
        ///     Zone ID of the alert
        /// </summary>
        public uint Zone { get; set; }

        /// <summary>
        ///     when the alert started
        /// </summary>
        public DateTime TimeStarted { get; set; }

        /// <summary>
        ///     when the alert ended. is <c>null</c> if the alert is currently in progress
        /// </summary>
        public DateTime? TimeEnded { get; set; }

        /// <summary>
        ///     the ID of the <see cref="PsMetagameEvent"/> this alert is of
        /// </summary>
        public int CensusMetagameEventType { get; set; }

        /// <summary>
        ///     the <see cref="PsMetagameEvent"/> with <see cref="PsMetagameEvent.ID"/> of <see cref="CensusMetagameEventType"/>,
        ///     or <c>null</c> if Honu could not find it
        /// </summary>
        public PsMetagameEvent? MetagameEvent { get; set; }

        /// <summary>
        ///     duration of the alert in milliseconds
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        ///     what bracket this alert is in, which is basically how many full platoons could each side field
        /// </summary>
        public int Bracket { get; set; } = -1;

        /// <summary>
        ///     the actual player count of players currently online and participating in the alert
        /// </summary>
        public int PlayerCount { get; set; }

        /// <summary>
        ///     the results object
        /// </summary>
        public PS2AlertResult Result { get; set; } = new();

    }

    public class PS2AlertResult {

        /// <summary>
        ///     the percentage as an integer of how many territories the VS has
        /// </summary>
        public int VS { get; set; }

        /// <summary>
        ///     the percentage as an integer of how many territories the NC has
        /// </summary>
        public int NC { get; set; }

        /// <summary>
        ///     the percentage as an integer of how many territories the TR has
        /// </summary>
        public int TR { get; set; }

        /// <summary>
        ///     did this alert end in a draw?
        /// </summary>
        public bool Draw { get; set; }

        /// <summary>
        ///     the faction ID that won the alert
        /// </summary>
        public short? Victor { get; set; }

    }

}
