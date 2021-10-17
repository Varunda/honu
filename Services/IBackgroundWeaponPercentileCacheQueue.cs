using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services {

    /// <summary>
    /// Queue of weapon percentile stats to generate
    /// </summary>
    public interface IBackgroundWeaponPercentileCacheQueue {

        /// <summary>
        /// Queue a new item to have it's percentile stats generated
        /// </summary>
        /// <param name="itemID">ID of the item</param>
        void Queue(string itemID);

        Task<string> Dequeue(CancellationToken cancel);

    }
}
