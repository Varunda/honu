using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Realtime {

    public class WorldTagManager {

        private const string SERVICE_NAME = "WorldTag";

        private readonly ILogger<WorldTagManager> _Logger;
        private readonly WorldTagDbStore _TagDb;

        private readonly Dictionary<short, WorldTagEntry> _Entries = new Dictionary<short, WorldTagEntry>();
        private readonly Dictionary<short, List<WorldTagEntry>> _RecentEntries = new Dictionary<short, List<WorldTagEntry>>();

        public WorldTagManager(ILogger<WorldTagManager> logger,
            WorldTagDbStore tagDb) {

            _Logger = logger;
            _TagDb = tagDb;
        }

        /// <summary>
        ///     Get the current character ID of the character who is "It" in a world
        /// </summary>
        /// <param name="worldID">ID of the world to get the character ID of</param>
        /// <returns>The tag entry for the character who is currently the kill target</returns>
        public WorldTagEntry? GetByWorld(short worldID) {
            lock (_Entries) {
                if (_Entries.TryGetValue(worldID, out WorldTagEntry? entry) == true) {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        ///     Get the most recent world tag entries
        /// </summary>
        /// <param name="worldID">ID of the world to get the recent tag entries of</param>
        public List<WorldTagEntry> GetRecent(short worldID) {
            lock (_RecentEntries) {
                if (_RecentEntries.TryGetValue(worldID, out List<WorldTagEntry>? recent) == false) {
                    return new List<WorldTagEntry>();
                }
                return recent;
            }
        }

        public async Task OnKillHandler(KillEvent ev) {
            WorldTagEntry? target = GetByWorld(ev.WorldID);

            if (target == null) {
                _Logger.LogDebug($"{SERVICE_NAME}> World {ev.WorldID} has no kill target, setting to attacker {ev.AttackerCharacterID}");
                SetKillTarget(ev.WorldID, ev.AttackerCharacterID, ev.Timestamp);
                return;
            }

            if (ev.AttackerCharacterID == ev.KilledCharacterID) { // Skip suicides
                return;
            }

            // Not doing anything timer
            if ((ev.Timestamp - target.LastKill) > TimeSpan.FromMinutes(5)) {
            //if ((ev.Timestamp - target.LastKill) > TimeSpan.FromSeconds(30)) {
                target.LastKill = ev.Timestamp;
                target.WasKilled = false;
                await _TagDb.Insert(target);

                lock (_Entries) {
                    _Entries.Remove(ev.WorldID);
                }

                _Logger.LogDebug($"{SERVICE_NAME}> World {ev.WorldID}, last target got a kill 5 mins ago, restarting chain");
                return;
            }

            if (ev.KilledCharacterID == target.CharacterID) {
                _Logger.LogDebug($"{SERVICE_NAME}> World {ev.WorldID}, IT was killed by {ev.KilledCharacterID}, had {target.Kills} kills");

                target.TargetKilled = ev.Timestamp;
                target.WasKilled = true;
                await _TagDb.Insert(target);

                SetKillTarget(ev.WorldID, ev.AttackerCharacterID, ev.Timestamp);
            } else if (ev.AttackerCharacterID == target.CharacterID) {
                ++target.Kills;
                target.LastKill = DateTime.UtcNow;
                //_Logger.LogDebug($"{SERVICE_NAME}> World {ev.WorldID}, target got a kill, now has {target.Kills} kills");
            }
        }

        public async Task OnLogoutHandler(string charID, DateTime when) {
            WorldTagEntry? entry = null;

            lock (_Entries) {
                foreach (KeyValuePair<short, WorldTagEntry> iter in _Entries) {
                    if (iter.Value.CharacterID == charID) {
                        entry = iter.Value;
                        break;
                    }
                }
            }

            if (entry != null) {
                _Logger.LogDebug($"{SERVICE_NAME}> Kill target {entry.CharacterID} logged out");

                entry.TargetKilled = when;
                entry.WasKilled = false;
                await _TagDb.Insert(entry);

                lock (_Entries) {
                    _Entries.Remove(entry.WorldID);
                }
            }
        }

        /// <summary>
        ///     Set the kill target of a world. It will create a new entry if needed, or update the existing one
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <param name="charID">Character ID that is the target</param>
        /// <param name="timestamp">When the kill event happened</param>
        public void SetKillTarget(short worldID, string charID, DateTime timestamp) {
            lock (_RecentEntries) {
                if (_RecentEntries.TryGetValue(worldID, out _) == false) {
                    _RecentEntries.Add(worldID, new List<WorldTagEntry>());
                    //_Logger.LogDebug($"{SERVICE_NAME}> Added new recent entries for {worldID}");
                }
            }

            WorldTagEntry newEntry = new WorldTagEntry() {
                CharacterID = charID,
                WorldID = worldID,
                Timestamp = timestamp,
                LastKill = timestamp,
                Kills = 1
            };

            lock (_Entries) {
                _Entries[worldID] = newEntry;
            }

            List<WorldTagEntry> recent = new List<WorldTagEntry>();
            lock (_RecentEntries) {
                recent = _RecentEntries[worldID]; // Safe cause it's ensured to be created above
            }

            recent.Insert(0, newEntry); // Insert at first, newest at first

            if (recent.Count > 10) { // Remove the oldest entry
                recent.RemoveAt(10);
            }

            lock (_RecentEntries) {
                _RecentEntries[worldID] = recent;
            }
        }

    }
}
