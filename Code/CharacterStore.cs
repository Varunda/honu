using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Tracking;

namespace watchtower.Models {

    public class CharacterStore {

        private static CharacterStore _Instance = new CharacterStore();
        public static CharacterStore Get() { return _Instance; }

        public ConcurrentDictionary<string, TrackedPlayer> Players = new ConcurrentDictionary<string, TrackedPlayer>();

        /// <summary>
        ///     Get a <see cref="TrackedPlayer"/>, locking as needed
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
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
        ///     get a list of <see cref="TrackedPlayer"/>s from this <see cref="CharacterStore"/>
        ///     that match the filter from <paramref name="func"/>. this will lock <see cref="CharacterStore.Players"/>,
        ///     so a lock around it is not useful
        /// </summary>
        /// <param name="func">function used to perform the filtering</param>
        /// <returns>
        ///     a list of <see cref="TrackedPlayer"/>s that fulfill the filter from <paramref name="func"/>
        /// </returns>
        public List<TrackedPlayer> GetByFilter(Func<TrackedPlayer, bool> func) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("CharacterStore getByFilter");
            List<TrackedPlayer> players = new List<TrackedPlayer>();

            lock (Players) {
                foreach (KeyValuePair<string, TrackedPlayer> iter in Players) {
                    if (func(iter.Value) == true) {
                        players.Add(iter.Value);
                    }
                }
            }

            return players;
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
