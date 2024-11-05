using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Background queue for performing a character update
    /// </summary>
    public class CharacterUpdateQueue : BaseQueue<CharacterUpdateQueueEntry> {

        private CharacterUpdateQueueEntry? _Last;

        private readonly HashSet<string> _Pending = new HashSet<string>();

        public CharacterUpdateQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) { }

        /// <summary>
        ///     Add a character ID to be updated
        /// </summary>
        /// <param name="charID">ID of the character to be updated</param>
        public void Queue(string charID) {
            if (_Last?.CharacterID == charID) {
                return;
            }

            lock (_Pending) {
                if (_Pending.Contains(charID)) {
                    return;
                }

                _Pending.Add(charID);
            }

            _Items.Enqueue(new CharacterUpdateQueueEntry() { CharacterID = charID });
            _Signal.Release();
        }

        /// <summary>
        ///     Add a character to be updated. Instead of getting the <see cref="PsCharacter"/> to be updated
        ///     the passed <see cref="PsCharacter"/> is used instead, saving a Census call
        /// </summary>
        /// <param name="character">Character to be updated</param>
        public void Queue(PsCharacter character) {
            if (_Last?.CharacterID == character.ID) {
                return;
            }

            lock (_Pending) {
                if (_Pending.Contains(character.ID)) {
                    return;
                }

                _Pending.Add(character.ID);
            }

            _Items.Enqueue(new CharacterUpdateQueueEntry() { CharacterID = character.ID, CensusCharacter = character });
            _Signal.Release();
        }

        public new async Task<CharacterUpdateQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterUpdateQueueEntry? entry);

            lock (_Pending) {
                _Pending.Remove(entry!.CharacterID);
            }

            _Last = entry;
            ++_ProcessedCount;

            return entry!;
        }

        public CharacterUpdateQueueEntry? GetMostRecentDequeued() {
            return _Last;
        }

    }
}
