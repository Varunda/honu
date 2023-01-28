using System;
using System.Collections.Generic;
using watchtower.Models.Census;

namespace watchtower.Models.PSB {

    public class PsbCalendarEntry {

        /// <summary>
        ///     The input values used to parse this entry. Useful for debugging
        /// </summary>
        public List<string?> Input { get; set; } = new();

        /// <summary>
        ///     If this row in the calendar could be parsed to a valid entry.
        ///     If false, then <see cref="Error"/> contains more information
        /// </summary>
        public bool Valid { get; set; } = false;

        /// <summary>
        ///     Information about why this row failed to be parsed to a valid entry
        /// </summary>
        public string Error { get; set; } = "";

        /// <summary>
        ///     What groups are in this reservation
        /// </summary>
        public List<string> Groups { get; set; } = new();

        /// <summary>
        ///     The input list of base names. See <see cref="Bases"/> for the parsed values
        /// </summary>
        public List<string> BaseNames { get; set; } = new();

        /// <summary>
        ///     A parses list of bases
        /// </summary>
        public List<PsbCalendarBaseEntry> Bases { get; set; } = new();

        /// <summary>
        ///     When this reservation starts
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     When this reservation ends
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     Any notes provided about the reservation
        /// </summary>
        public string Notes { get; set; } = "";

    }

    public class PsbCalendarBaseEntry {

        /// <summary>
        ///     What name was on the calendar
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     What bases this entry possibly refers to
        /// </summary>
        public List<PsFacility> PossibleBases { get; set; } = new();

    }

}
