using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Realtime {

    public interface IEventHandler {

        /// <summary>
        ///     process a json token as an event
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        Task Process(JToken ev);

        /// <summary>
        ///     get the timestamp of the most recently processed event
        /// </summary>
        /// <returns></returns>
        DateTime MostRecentProcess();

    }
}
