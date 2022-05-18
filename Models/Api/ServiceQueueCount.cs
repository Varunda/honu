using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    public class ServiceQueueCount {

        public string QueueName { get; set; } = "";

        public int Count { get; set; }

        public double? Average { get; set; }

    }
}
