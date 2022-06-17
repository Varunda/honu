using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class CharacterStore {

        private static CharacterStore _Instance = new CharacterStore();
        public static CharacterStore Get() { return _Instance; }

        public ConcurrentDictionary<string, TrackedPlayer> Players = new ConcurrentDictionary<string, TrackedPlayer>();

        public TrackedPlayer? GetByCharacterID(string charID) {
            TrackedPlayer? player;

            lock (Players) {
                Players.TryGetValue(charID, out player);
            }

            return player;
        }

        public void SetByCharacterID(string charID, TrackedPlayer player) {
            lock (Players) {
                Players[charID] = player;
            }
        }

        /// <summary>
        ///     Get how many characters are currently online
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <returns></returns>
        public int GetWorldCount(short worldID) {
            lock (Players) {
                return Players.Where(iter => iter.Value.WorldID == worldID && iter.Value.Online == true).Count();
            }
        }

    }

}
