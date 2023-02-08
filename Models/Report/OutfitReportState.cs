namespace watchtower.Models.Report {

    /// <summary>
    ///     Has the states of making an outfit report
    /// </summary>
    public class OutfitReportState {

        /// <summary>
        ///     The initial state, nothing has happened yet
        /// </summary>
        public const string NOT_STARTED = "not_started";

        /// <summary>
        ///     The report only has a generator string, and it is yet to be parsed
        /// </summary>
        public const string PARSING_GENERATOR = "parsing_generator";

        /// <summary>
        ///     The report has a generator string, but doesn't know what characters are relevant yet,
        ///     it will next get the sessions from <see cref="OutfitReportParameters.CharacterIDs"/>
        ///     and <see cref="OutfitReportParameters.OutfitIDs"/> between the time ranges in the params
        /// </summary>
        public const string GETTING_SESSIONS = "getting_sessions";

        /// <summary>
        ///     The report has all the sessions that occured during the time, now Honu gets the kills
        ///     for all the characters what were tracked
        /// </summary>
        public const string GETTING_KILLDEATHS = "getting_killdeaths";

        /// <summary>
        ///     The report has gotten all the kill//deaths during the time, now Honu is getting the
        ///     exp events for all characters that were tracked
        /// </summary>
        public const string GETTING_EXP = "getting_exp";

        /// <summary>
        ///     The report has gotten all the exp evetns, now vehicle destroy time
        /// </summary>
        public const string GETTING_VEHICLE_DESTROY = "getting_vehicle_destroy";

        /// <summary>
        ///     
        /// </summary>
        public const string GETTING_ACHIEVEMENT_EARNED = "getting_achievement_earned";

        /// <summary>
        ///     Report has vehicle destroy, getting player control events
        /// </summary>
        public const string GETTING_PLAYER_CONTROL = "getting_player_control";

        /// <summary>
        ///     Report has all the events, get the facility control events
        /// </summary>
        public const string GETTING_FACILITY_CONTROL = "getting_facility_control";

        /// <summary>
        ///     Report has all the events Honu cares about, get ALL the possible
        ///     characters that could be displayed
        /// </summary>
        public const string GETTING_CHARACTERS = "getting_characters";

        /// <summary>
        ///     We get the items now woah
        /// </summary>
        public const string GETTING_ITEMS = "getting_items";

        public const string GETTING_OUTFITS = "getting_outfits";

        public const string GETTING_FACILITIES = "getting_facilities";

        public const string GETTING_RECONNETS = "getting_reconnects";

        public const string DONE = "done";

    }
}
