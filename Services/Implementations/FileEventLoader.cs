using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services {

    public class FileEventLoader : IFileEventLoader {

        private readonly ILogger<FileEventLoader> _Logger;

        public FileEventLoader(ILogger<FileEventLoader> logger) { 
            _Logger = logger;
        }

        public async Task Load(string filename) {
            try {
                string json = await File.ReadAllTextAsync(filename);

                JToken root = JToken.Parse(json);
                if (root == null) {
                    return;
                }
                if (root.Type != JTokenType.Array) {
                    _Logger.LogWarning($"Cannot load events: root type is not Array ({root.Type})");
                    return;
                }

                List<TrackedPlayer>? players;

                try {
                    players = ((JArray)root).ToObject<List<TrackedPlayer>>();
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Failed to parse JSON from {filename}", filename);
                    return;
                }

                if (players == null) {
                    _Logger.LogWarning($"players was null");
                    return;
                }

                lock (CharacterStore.Get().Players) {
                    foreach (TrackedPlayer player in players) {
                        if (CharacterStore.Get().Players.TryGetValue(player.ID, out TrackedPlayer? existingPlayer) == true) {
                            if (existingPlayer == null) {
                                continue;
                            }

                            existingPlayer.Kills.AddRange(player.Kills);
                            existingPlayer.Deaths.AddRange(player.Deaths);
                            existingPlayer.Heals.AddRange(player.Heals);
                            existingPlayer.Revives.AddRange(player.Revives);
                            existingPlayer.Repairs.AddRange(player.Repairs);
                            existingPlayer.Resupplies.AddRange(player.Resupplies);
                            existingPlayer.Spawns.AddRange(player.Spawns);
                        } else {
                            CharacterStore.Get().Players.TryAdd(player.ID, player);
                        }
                    }
                }

                //await _Characters.CacheBlock(players.Select(i => i.ID).ToList());
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to load events");
            }
        }

        public async Task Save(string filename) {
            Dictionary<string, TrackedPlayer> players;
            lock (CharacterStore.Get().Players) {
                players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
            }

            List<TrackedPlayer> list = new List<TrackedPlayer>(players.Count);
            foreach (KeyValuePair<string, TrackedPlayer> entry in players) {
                list.Add(entry.Value);
            }

            JToken json = JToken.FromObject(list);

            await File.WriteAllTextAsync("PreviousEvents.json", json.ToString());
        }

    }
}
