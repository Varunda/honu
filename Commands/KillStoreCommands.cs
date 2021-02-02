using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Models;
using watchtower.Models.Events;

namespace watchtower.Commands {

    [Command]
    public class KillStoreCommand {

        private readonly ILogger<KillStoreCommand> _Logger;
        private readonly ICharacterCollection _Characters;

        public KillStoreCommand(IServiceProvider services) {
            _Logger = (ILogger<KillStoreCommand>)services.GetService(typeof(ILogger<KillStoreCommand>));

            _Characters = (ICharacterCollection)services.GetService(typeof(ICharacterCollection));
        }

        public void Flush() {
            lock (KillStore.Get().Players) {
                KillStore.Get().Players.Clear();
            }
            _Logger.LogInformation($"Flushed KillStore");
        }

        public async Task Clear(string name) {
            Character? c = _Characters.GetCache().Find(iter => iter.Name.ToLower() == name.ToLower());
            if (c == null) {
                c = await _Characters.GetByNameAsync(name);
            }
            if (c == null) {
                _Logger.LogWarning($"Failed to find {name}");
                return;
            }

            lock (KillStore.Get().Players) {
                if (KillStore.Get().Players.TryRemove(c.ID, out _) == true) {
                    _Logger.LogInformation($"Removed {c.Name}");
                } else {
                    _Logger.LogInformation($"Didn't remove {c.Name}");
                }
            }
        }

        public async Task Add(string name, int kills, int deaths) {
            List<Character> characters = _Characters.GetCache();

            Character? c = characters.Find(iter => iter.Name.ToLower() == name.ToLower());
            if (c == null) {
                c = await _Characters.GetByNameAsync(name);
            }
            if (c == null) {
                _Logger.LogWarning($"Failed to find {name}");
                return;
            }

            Int64 currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            lock (KillStore.Get().Players) {
                if (KillStore.Get().Players.TryGetValue(c.ID, out TrackedPlayer? p) == true) {
                    if (p == null) {
                        return;
                    }
                } else {
                    p = new TrackedPlayer() {
                        ID = c.ID,
                        FactionID = c.FactionID,
                    };
                    KillStore.Get().Players.TryAdd(p.ID, p);
                }

                for (int i = 0; i < kills; ++ i) {
                    p.Kills.Add((int) currentTime);
                }
                _Logger.LogInformation($"Added {kills} kills to {name}");

                for (int i = 0; i < deaths; ++ i) {
                    p.Deaths.Add((int) currentTime);
                }
                _Logger.LogInformation($"Added {kills} deaths to {name}");
            }
        }

    }
}
