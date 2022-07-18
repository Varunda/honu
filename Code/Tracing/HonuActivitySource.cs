using System.Diagnostics;

namespace watchtower.Code.Tracking {

    public static class HonuActivitySource {

        public static readonly string ActivitySourceName = "Honu";

        /// <summary>
        ///     Root activity source timing is done from
        /// </summary>
        public static readonly ActivitySource Root = new ActivitySource(ActivitySourceName);

    }
}
