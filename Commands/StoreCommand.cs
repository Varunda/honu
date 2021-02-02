using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Models;

namespace watchtower.Commands {

    [Command]
    public class StoreCommand {

        private readonly ILogger<StoreCommand> _Logger;
        private readonly ICharacterCollection _Characters;

        public StoreCommand(IServiceProvider services) {
            _Logger = (ILogger<StoreCommand>)services.GetService(typeof(ILogger<StoreCommand>));

            _Characters = (ICharacterCollection)services.GetService(typeof(ICharacterCollection));
        }

        public async Task Print(string nameOrId) {
            Character? c = null;
            if (nameOrId.Length == 19) {
                c = await _Characters.GetByIDAsync(nameOrId);
            } else {
                c = await _Characters.GetByNameAsync(nameOrId);
            }

            if (c == null) {
                _Logger.LogWarning($"Failed to find {nameOrId}");
                return;
            }

            string charId = c.ID;

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
                + $"\tRepairs: {player.Repairs.Count}\n"
            );
        }

    }
}
