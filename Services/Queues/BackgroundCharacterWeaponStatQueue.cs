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
    ///     Background queue for performing a character update
    /// </summary>
    public class BackgroundCharacterWeaponStatQueue {

        private ConcurrentQueue<CharacterUpdateQueueEntry> _Items = new ConcurrentQueue<CharacterUpdateQueueEntry>();
        private SemaphoreSlim _Signal = new SemaphoreSlim(0);

        /// <summary>
        ///     Add a character ID to be updated
        /// </summary>
        /// <param name="charID">ID of the character to be updated</param>
        public void Queue(string charID) {
            _Items.Enqueue(new CharacterUpdateQueueEntry() { CharacterID = charID });
            _Signal.Release();
        }

        /// <summary>
        ///     Add a character to be updated. Instead of getting the <see cref="PsCharacter"/> to be updated
        ///     the passed <see cref="PsCharacter"/> is used instead, saving a Census call
        /// </summary>
        /// <param name="character">Character to be updated</param>
        public void Queue(PsCharacter character) {
            _Items.Enqueue(new CharacterUpdateQueueEntry() { CharacterID = character.ID, CensusCharacter = character });
            _Signal.Release();
        }

        public void Queue(CharacterUpdateQueueEntry entry) {
            _Items.Enqueue(entry);
            _Signal.Release();
        }

        /// <summary>
        ///     Get a <see cref="CharacterUpdateQueueEntry"/> from the queue. This is a blocking call,
        ///     and will not be completed until something can be released from the queue
        /// </summary>
        /// <param name="cancel">Cancel token</param>
        public async Task<CharacterUpdateQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterUpdateQueueEntry? charID);

            return charID!;
        }

        /// <summary>
        ///     How many items are currently in the queue
        /// </summary>
        public int Count() {
            return _Items.Count();
        }

    }
}
