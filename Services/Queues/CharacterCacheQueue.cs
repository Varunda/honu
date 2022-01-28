using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for caching <see cref="PsCharacter"/>s in memory
    /// </summary>
    public class CharacterCacheQueue : BaseQueue<CharacterFetchQueueEntry> {

        /// <summary>
        ///     Queue a new character for caching
        /// </summary>
        /// <param name="charID">ID of the character</param>
        public void Queue(string charID) {
            _Items.Enqueue(new CharacterFetchQueueEntry() {
                CharacterID = charID,
                Store = true
            });
            _Signal.Release();
        }

    }
}
