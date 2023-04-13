using System;
using System.Collections.Generic;
using watchtower.Models.Events;

namespace watchtower.Models.Report {

    /// <summary>
    ///     Parameters used to generate a report
    /// </summary>
    public class OutfitReportParameters {

        /// <summary>
        ///     Empty by default, if not empty, this has the ID of the parameters to load from the report DB
        /// </summary>
        public Guid ID { get; set; } = Guid.Empty;

        /// <summary>
        ///     When these parameters were first used
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     String used to generate these parameters
        /// </summary>
        public string Generator { get; set; } = "";

        /// <summary>
        ///     What team events will be limited to
        /// </summary>
        public short TeamID { get; set; } = -1;

        /// <summary>
        ///     Optional, what zone to filter events to
        /// </summary>
        public uint? ZoneID { get; set; } = null;

        /// <summary>
        ///     Will kills against the same team be included?
        /// </summary>
        public bool IncludeTeamkills { get; set; } = false;

        /// <summary>
        ///     Will deaths that were revived be included?
        /// </summary>
        public bool IncludeRevivedDeaths { get; set; } = false;

        /// <summary>
        ///     Will deaths from the same team be included?
        /// </summary>
        public bool IncludeTeamkilled { get; set; } = false;

        /// <summary>
        ///     Will achivements earned during the period by tracked characters be included?
        /// </summary>
        public bool IncludeAchievementsEarned { get; set; } = false;

        /// <summary>
        ///     Will <see cref="ExpEvent"/> with a <see cref="ExpEvent.OtherID"/> of a tracked character be included?
        /// </summary>
        public bool IncludeOtherIdExpEvents { get; set; } = false;

        /// <summary>
        ///     What character IDs were included, this does not include the characters in outfits,
        ///     only the ones added by the generator string
        /// </summary>
        public List<string> CharacterIDs { get; set; } = new();

        /// <summary>
        ///     What outfits will be included in the report
        /// </summary>
        public List<string> OutfitIDs { get; set; } = new();

        /// <summary>
        ///     What players will be ignored when generating the stats
        /// </summary>
        public List<string> IgnoredPlayers { get; set; } = new();

        /// <summary>
        ///     When thie outfit report starts
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        ///     When this outfit report ends
        /// </summary>
        public DateTime PeriodEnd { get; set; }

    }
}
