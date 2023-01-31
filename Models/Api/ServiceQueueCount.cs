using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    public class ServiceQueueCount {

        /// <summary>
        ///     Name of the queue
        /// </summary>
        public string QueueName { get; set; } = "";

        /// <summary>
        ///     How many items are currently in this queue
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        ///     How many items have been removed from this queue
        /// </summary>
        public long Processed { get; set; }

        /// <summary>
        ///     The average time to process recent items in milliseconds
        /// </summary>
        public double? Average { get; set; }

        /// <summary>
        ///     The min time to process recent items in milliseconds
        /// </summary>
        public double? Min { get; set; }

        /// <summary>
        ///     The median time to process recent items in milliseconds
        /// </summary>
        public double? Median { get; set; }

        /// <summary>
        ///     The max time to process recent items in milliseconds
        /// </summary>
        public double? Max { get; set; }

    }
}
