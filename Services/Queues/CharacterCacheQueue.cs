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

        private readonly ConcurrentDictionary<string, DateTime> _CachedAt = new ();

        /// <summary>
        ///     Queue a new character for caching
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <param name="environment">What environment this character is in, see </param>
        public void Queue(string charID, CensusEnvironment environment) {
            if (charID == "0") {
                return;
            }

            if (_Pending.Contains(charID)) {
                return;
            }

            _Items.Enqueue(new CharacterFetchQueueEntry() {
                CharacterID = charID,
                Store = true,
                Environment = environment
            });

            lock (_Pending) {
                _Pending.Add(charID);
            }

            _Signal.Release();
        }

        public new async Task<CharacterFetchQueueEntry> Dequeue(CancellationToken cancel) {
            await _Signal.WaitAsync(cancel);
            _Items.TryDequeue(out CharacterFetchQueueEntry? entry);

            lock (_Pending) {
                _Pending.Remove(entry!.CharacterID);
            }

            return entry!;
        }

    }
}
