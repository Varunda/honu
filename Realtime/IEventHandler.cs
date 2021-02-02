using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Realtime {

    public interface IEventHandler {

        void Process(JToken ev);

    }
}
