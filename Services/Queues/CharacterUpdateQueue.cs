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
    public class CharacterUpdateQueue : BaseQueue<CharacterUpdateQueueEntry> {

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

    }
}
