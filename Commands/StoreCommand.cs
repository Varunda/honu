using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Services;

namespace watchtower.Commands {

    [Command]
    public class StoreCommand {

        private readonly ILogger<StoreCommand> _Logger;
        private readonly ICharacterCollection _Characters;
        private readonly IFileEventLoader _Loader;

        public StoreCommand(IServiceProvider services) {
            _Logger = (ILogger<StoreCommand>)services.GetService(typeof(ILogger<StoreCommand>));

            _Characters = (ICharacterCollection)services.GetService(typeof(ICharacterCollection));
            _Loader = (IFileEventLoader)services.GetService(typeof(IFileEventLoader));
        }

        public async Task Print(string nameOrId) {
            Character? c;
            if (nameOrId.Length == 19) {
                c = await _Characters.GetByIDAsync(nameOrId);
            } else {
                c = await _Characters.GetByNameAsync(nameOrId);
            }

            if (c == null) {
                _Logger.LogWarning($"Failed to find {nameOrId}");
                return;
            }

            TrackedPlayer? player;
            bool found;
            lock (CharacterStore.Get().Players) {
                found = CharacterStore.Get().Players.TryGetValue(c.ID, out player);
            }

            if (found == false || player == null) {
                _Logger.LogWarning($"{c.Name}/{c.ID} not tracked");
                return;
            }

            _Logger.LogInformation(
                $"Character {nameOrId}:\n"
                + $"\tName: {c.Name}\n"
                + $"\tID: {c.ID}\n"
                + $"\tFactionID: {c.FactionID}\n"
                + $"\tKills: {player.Kills.Count}\n"
                + $"\tDeaths: {player.Deaths.Count}\n"
                + $"\tHeals: {player.Heals.Count}\n"
                + $"\tRevives: {player.Revives.Count}\n"
                + $"\tRepairs: {player.Repairs.Count}\n"
                + $"\tResupplies: {player.Resupplies.Count}\n"
            );
        }

        public async Task Online() {
            Dictionary<string, TrackedPlayer> players;
            lock (CharacterStore.Get().Players) {
                players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
            }

            int trOnline = 0;
            int trTotal = 0;
            int ncOnline = 0;
            int ncTotal = 0;
            int vsOnline = 0;
            int vsTotal = 0;

            foreach (KeyValuePair<string, TrackedPlayer> entry in players) {
                if (entry.Value.FactionID == Faction.TR) {
                    if (entry.Value.Online == true) {
                        ++trOnline;
                    }
                    ++trTotal;
                } else if (entry.Value.FactionID == Faction.NC) {
                    if (entry.Value.Online == true) {
                        ++ncOnline;
                    }
                    ++ncTotal;
                }  else if (entry.Value.FactionID == Faction.VS) {
                    if (entry.Value.Online == true) {
                        ++vsOnline;
                    }
                    ++vsTotal;
                }
            }

            _Logger.LogInformation($"Stats:\n"
                + $"\tOnline: {trOnline + ncOnline + vsOnline} / Total: {trTotal + ncTotal + vsTotal}\n"
                + $"\tVS: {vsOnline} / {vsTotal}\n"
                + $"\tNC: {ncOnline} / {ncTotal}\n"
                + $"\tTR: {trOnline} / {trTotal}\n"
            );
        }

        public async Task Outfit(string tag) {
            Dictionary<string, TrackedPlayer> players;
            lock (CharacterStore.Get().Players) {
                players = new Dictionary<string, TrackedPlayer>(CharacterStore.Get().Players);
            }

            List<Character> characters = new List<Character>(_Characters.GetCache());

            List<string> charIDs = new List<string>();

            foreach (Character c in characters) {
                if (c.OutfitTag?.ToLower() == tag.ToLower()) {
                    charIDs.Add(c.ID);
                }
            }

            int online = 0;
            int total = 0;
            int kills = 0;
            int killsOnline = 0;
            int deaths = 0;
            int deathsOnline = 0;
            int heals = 0;
            int revives = 0;
            int resupplies = 0;
            int repairs = 0;

            foreach (string charID in charIDs) {
                if (players.TryGetValue(charID, out TrackedPlayer? p) == true) {
                    if (p == null) {
                        continue;
                    }

                    if (p.Online == true) {
                        ++online;
                        killsOnline += p.Kills.Count;
                        deathsOnline += p.Deaths.Count;
                    }

                    ++total;
                    kills += p.Kills.Count;
                    deaths += p.Deaths.Count;
                    heals += p.Heals.Count;
                    revives += p.Revives.Count;
                    repairs += p.Repairs.Count;
                    resupplies += p.Resupplies.Count;
                }
            }

            _Logger.LogInformation(
                $"Outfit: {tag}\n"
                + $"\tOnline: {online} / Total: {total}\n"
                + $"\tKills: {kills} / {killsOnline}\n"
                + $"\tDeaths: {deaths} / {deathsOnline}\n"
                + $"\tHeals: {heals}\n"
                + $"\tRevives: {revives}\n"
                + $"\tRepairs: {repairs}\n"
                + $"\tResupplies: {resupplies}\n"
            );
        }

        public async Task Save() {
            await _Loader.Save("PreviousEvents.json");

            _Logger.LogInformation($"Saved previous events");
        }

        public async Task Load() {
            await _Loader.Load("PreviousEvents.json");

            _Logger.LogInformation($"Loaded previous events");
        }

    }
}
