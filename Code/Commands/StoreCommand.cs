using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Commands {

    [Command]
    public class StoreCommand {

        private readonly ILogger<StoreCommand> _Logger;
        private readonly ICharacterCollection _Characters;
        private readonly IFileEventLoader _Loader;
        private readonly IExpEventDbStore _ExpEventDb;

        public StoreCommand(IServiceProvider services) {
            _Logger = (ILogger<StoreCommand>)services.GetService(typeof(ILogger<StoreCommand>));

            _Loader = (IFileEventLoader)services.GetService(typeof(IFileEventLoader));
            _ExpEventDb = services.GetRequiredService<IExpEventDbStore>();
            _Characters = services.GetRequiredService<ICharacterCollection>();
        }

        public async Task Exp(int id1, int id2) {
            ExpEntryOptions options = new ExpEntryOptions() {
                WorldID = 1,
                FactionID = null,
                ExperienceIDs = new List<int>() { id1, id2 },
                Interval = 120
            };

            List<ExpDbEntry> entries = await _ExpEventDb.GetEntries(options);

            foreach (ExpDbEntry entry in entries) {
                _Logger.LogInformation($"{entry.CharacterID} => {entry.Count}");
            }
        }

        public async Task Print(string nameOrId) {
            PsCharacter? c;
            if (nameOrId.Length == 19) {
                c = await _Characters.GetByID(nameOrId);
            } else {
                c = await _Characters.GetByName(nameOrId);
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
