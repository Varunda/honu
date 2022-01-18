using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Queues;

namespace watchtower.Services {

    public interface IBackgroundCharacterCacheQueue {

        /// <summary>
        /// Queue a new character for getting
        /// </summary>
        /// <param name="charID">ID of the character</param>
        void Queue(string charID);

        void Queue(CharacterFetchQueueEntry entry);

        Task<CharacterFetchQueueEntry> DequeueAsync(CancellationToken cancel);

        /// <summary>
        ///     Get current queue length
        /// </summary>
        /// <returns></returns>
        int Count();

    }
}
