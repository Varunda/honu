using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    public class PriorityCharacterUpdateQueue : BaseQueue<CharacterUpdateQueueEntry> {

        private CharacterUpdateQueueEntry? _Last;

        private readonly HashSet<string> _Pending = new HashSet<string>();

        public PriorityCharacterUpdateQueue(ILoggerFactory factory) : base(factory) { }

        /// <summary>
        ///     Add a character ID to be updated
        /// </summary>
        /// <param name="charID">ID of the character to be updated</param>
        public void Queue(string charID) {
            if (_Last?.CharacterID == charID) {
                _Logger.LogDebug($"not queueing {charID}: _Last was this one");
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
            if (_Last?.CharacterID == character.ID) {
                _Logger.LogDebug($"not queueing {character.ID}: _Last was this one");
                return;
            }

            lock (_Pending) {
                if (_Pending.Contains(character.ID)) {
                    _Logger.LogDebug($"not queueing {character.ID}: In _Pending");
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
