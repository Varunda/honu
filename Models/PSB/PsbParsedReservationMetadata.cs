namespace watchtower.Models.PSB {

    public class PsbParsedReservationMetadata {

        /// <summary>
        ///     ID of the discord message this parsed reservation is for
        /// </summary>
        public ulong MessageID { get; set; }

        /// <summary>
        ///     ID of the google drive sheet that contains the accounts
        /// </summary>
        public string? AccountSheetId { get; set; }

        /// <summary>
        ///     ID of the discord user that approved the accounts
        /// </summary>
        public ulong? AccountSheetApprovedById { get; set; }

        /// <summary>
        ///     ID of the discord user that approved the base bookings
        /// </summary>
        public ulong? BookingApprovedById { get; set; }

    }
}
