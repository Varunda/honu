using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Census;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    /// <summary>
    ///     Queue for caching <see cref="PsCharacter"/>s in memory
    /// </summary>
    public class CharacterCacheQueue : BaseQueue<CharacterFetchQueueEntry> {

        private readonly HashSet<string> _Pending = new HashSet<string>();

        public CharacterCacheQueue(ILoggerFactory factory) : base(factory) { }

        /// <summary>
        ///     Queue a new character for caching
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="environment">What environment this character is in, see </param>
        public void Queue(string charID, CensusEnvironment environment) {
            if (charID == "0") {
                return;
            }

            // this goes before the item is actually queued, as it's possible for the entry to not be
            // pending but queued otherwise
            lock (_Pending) {
                if (_Pending.Contains(charID)) {
                    return;
                }

                _Pending.Add(charID);
            }

            _Items.Enqueue(new CharacterFetchQueueEntry() {
                CharacterID = charID,
                Store = true,
                Environment = environment
            });

            _Signal.Release();
        }

        public new async Task<CharacterFetchQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterFetchQueueEntry? entry);

            lock (_Pending) {
                _Pending.Remove(entry!.CharacterID);
            }

            ++_ProcessedCount;

            return entry!;
        }

    }
}
