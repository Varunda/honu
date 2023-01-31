using System.Collections.Generic;

namespace watchtower.Services.Queues {

    public interface IProcessQueue {

        List<long> GetProcessTime();

        int Count();

        long Processed();

    }
}
