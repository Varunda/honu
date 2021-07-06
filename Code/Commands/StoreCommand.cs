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
        private readonly IExpEventDbStore _ExpEventDb;
        private readonly IKillEventDbStore _KillEventDb;

        public StoreCommand(IServiceProvider services) {
            _Logger = (ILogger<StoreCommand>)services.GetService(typeof(ILogger<StoreCommand>));

            _ExpEventDb = services.GetRequiredService<IExpEventDbStore>();
            _KillEventDb = services.GetRequiredService<IKillEventDbStore>();
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
                _Logger.LogInformation($"{entry.ID} => {entry.Count}");
            }
        }

        public async Task Top(int factionID) {
            KillDbOptions options = new KillDbOptions() {
                FactionID = (short)factionID,
                Interval = 120,
                WorldID = 1
            };

            List<KillDbEntry> entries = await _KillEventDb.GetTopKillers(options);

            foreach (KillDbEntry entry in entries) {
                _Logger.LogInformation($"{entry.CharacterID} => {entry.Kills}/{entry.Deaths}");
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
                + $"\tFactionID: {c.FactionID}"
            );
        }

        public void Count() {
            lock (CharacterStore.Get().Players) {
                int totalCount = CharacterStore.Get().Players.Count;

                int Count(Func<KeyValuePair<string, TrackedPlayer>, bool> predicate) {
                    return CharacterStore.Get().Players.Where(predicate).Count();
                }

                int vsCount = Count(iter => iter.Value.TeamID == Faction.VS);
                int ncCount = Count(iter => iter.Value.TeamID == Faction.NC);
                int trCount = Count(iter => iter.Value.TeamID == Faction.TR);
                int nsCount = Count(iter => iter.Value.TeamID == Faction.NS);

                int indarCount = Count(iter => iter.Value.ZoneID == Zone.Indar);
                int esamirCount = Count(iter => iter.Value.ZoneID == Zone.Esamir);
                int hossinCount = Count(iter => iter.Value.ZoneID == Zone.Hossin);
                int amerishCount = Count(iter => iter.Value.ZoneID == Zone.Amerish);
                int otherCount = Count(iter => iter.Value.ZoneID == "0" || iter.Value.ZoneID == "-1");

                _Logger.LogInformation($"Characters being tracked:\n"
                    + $"Total: {totalCount}\n"
                    + $"VS: {vsCount}\n"
                    + $"NC: {ncCount}\n"
                    + $"TR: {trCount}\n"
                    + $"NS: {nsCount}\n"
                    + $"Indar: {indarCount}\n"
                    + $"Hossin: {hossinCount}\n"
                    + $"Amerish: {amerishCount}\n"
                    + $"Esamir: {esamirCount}\n"
                    + $"Other: {otherCount}\n"
                );
            }
        }

    }
}
