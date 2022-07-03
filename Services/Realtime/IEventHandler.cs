using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Realtime {

    public interface IEventHandler {

        Task Process(JToken ev);

        DateTime MostRecentProcess();

    }
}
