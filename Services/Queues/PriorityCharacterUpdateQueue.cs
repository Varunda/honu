using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Metrics;

namespace watchtower.Services.Queues {

    public class PriorityCharacterUpdateQueue : BaseQueue<CharacterUpdateQueueEntry> {

        private readonly HashSet<string> _Pending = new HashSet<string>();

        public PriorityCharacterUpdateQueue(ILoggerFactory factory, QueueMetric metrics) : base(factory, metrics) { }

        /// <summary>
        ///     Add a character ID to be updated
        /// </summary>
        /// <param name="charID">ID of the character to be updated</param>
        public void Queue(string charID) {
            if (!IsNumeric(charID)) {
                _Logger.LogDebug($"{charID} is not only numbers, not adding to queue");
                return;
            }

            lock (_Pending) {
                if (_Pending.Contains(charID)) {
                    _Logger.LogDebug($"not queueing {charID}: In _Pending");
                    return;
                }

                _Pending.Add(charID);
            }

            _Logger.LogDebug($"added {charID} to queue");

            _Items.Enqueue(new CharacterUpdateQueueEntry() { CharacterID = charID });
            _Signal.Release();
        }

        /// <summary>
        ///     Add a character to be updated. Instead of getting the <see cref="PsCharacter"/> to be updated
        ///     the passed <see cref="PsCharacter"/> is used instead, saving a Census call
        /// </summary>
        /// <param name="character">Character to be updated</param>
        public void Queue(PsCharacter character) {
            lock (_Pending) {
                if (_Pending.Contains(character.ID)) {
                    _Logger.LogDebug($"not queueing {character.ID}: In _Pending");
                    return;
                }

                _Pending.Add(character.ID);
            }

            CharacterUpdateQueueEntry entry = new();
            entry.CharacterID = character.ID;
            entry.CensusCharacter = character;

            Queue(entry);
        }

        public new async Task<CharacterUpdateQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterUpdateQueueEntry? entry);

            lock (_Pending) {
                _Pending.Remove(entry!.CharacterID);
            }

            ++_ProcessedCount;

            return entry!;
        }

        private static bool IsNumeric(string str) {
            foreach (char c in str) {
                if (char.IsDigit(c) == false) {
                    return false;
                } 
            }
            return true;
        }

    }

}
