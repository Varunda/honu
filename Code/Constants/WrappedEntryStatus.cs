namespace watchtower.Code.Constants {

    public class WrappedEntryStatus {

        /// <summary>
        ///     Unknown status
        /// </summary>
        public const int UNKNOWN = -1;

        /// <summary>
        ///     This wrapped entry was not started, or is currently in queue
        /// </summary>
        public const int NOT_STARTED = 1;

        /// <summary>
        ///     This wrapped entry is currently being processed
        /// </summary>
        public const int IN_PROGRESS = 2;

        /// <summary>
        ///     This wrapped entry was successfully processed with no errors
        /// </summary>
        public const int DONE = 3;

    }
}
