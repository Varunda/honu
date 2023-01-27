namespace watchtower.Models.PSB {

    public class JaegerNsaOptions {

        /// <summary>
        ///     How many seconds in between each run 
        /// </summary>
        public int IntervalSeconds { get; set; } = 60 * 5; // 60 seconds * 5 minutes

        /// <summary>
        ///     ID of the Discord role to @ if a known dev account is spotted signing in
        /// </summary>
        public ulong? AlertRoleID { get; set; }

    }
}
