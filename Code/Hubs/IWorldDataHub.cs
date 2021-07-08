using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Code.Hubs {

    /// <summary>
    /// Strongly typed signalrR hub 
    /// </summary>
    public interface IWorldDataHub {

        /// <summary>
        ///     Send the <see cref="WorldData"/> to all clients who connect to a server's world data
        /// </summary>
        /// <param name="data">Data to be sent</param>
        Task UpdateData(WorldData data);

    }
}
