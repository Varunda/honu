using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code;
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

        private readonly CharacterCollection _Characters;
        private readonly IExpEventDbStore _ExpEventDb;
        private readonly IKillEventDbStore _KillEventDb;
        private readonly SessionDbStore _SessionDb;

        public StoreCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<StoreCommand>>();

            _ExpEventDb = services.GetRequiredService<IExpEventDbStore>();
            _KillEventDb = services.GetRequiredService<IKillEventDbStore>();
            _Characters = services.GetRequiredService<CharacterCollection>();
            _SessionDb = services.GetRequiredService<SessionDbStore>();
        }

        public async Task Exp(short factionID, int id1, int id2) {
            ExpEntryOptions options = new ExpEntryOptions() {
                WorldID = 1,
                FactionID = factionID,
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

            List<Session> sessions = await _SessionDb.GetByCharacterID(c.ID, 120);

            double sessionLength = sessions.Sum(iter => ((iter.End ?? DateTime.UtcNow) - iter.Start).TotalSeconds);

            _Logger.LogInformation(
                $"Character {nameOrId}:\n"
                + $"\tName: {c.Name}\n"
                + $"\tID: {c.ID}\n"
                + $"\tFactionID: {c.FactionID}\n"
                + $"\tTeamID: {player.TeamID}\n"
                + $"\tWorldID: {player.WorldID}\n"
                + $"\tZoneID: {player.ZoneID}\n"
                + $"\tOnline: {player.Online}\n"
                + $"\tSessions: {sessions.Count} sessions, {sessionLength} seconds\n"
                + $"{String.Join("\n", sessions.Select(iter => $"\t\t{iter.Start} - {iter.End} {((iter.End ?? DateTime.UtcNow) - iter.Start).TotalSeconds}"))}"
            );
        }

        public void Count(short worldID) {
            lock (CharacterStore.Get().Players) {
                int Count(Func<KeyValuePair<string, TrackedPlayer>, bool> predicate) {
                    return CharacterStore.Get().Players.Count(predicate);
                }

                int totalCount = Count(iter => iter.Value.WorldID == worldID);

                totalCount = CharacterStore.Get().Players.Where(iter => iter.Value.WorldID == worldID).Count();

                int vsCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.TeamID == Faction.VS);
                int vsCountWorld = Count(iter => iter.Value.TeamID == Faction.VS);
                int ncCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.TeamID == Faction.NC);
                int trCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.TeamID == Faction.TR);
                int nsCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.TeamID == Faction.NS);

                int indarCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.ZoneID == Zone.Indar);
                int esamirCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.ZoneID == Zone.Esamir);
                int hossinCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.ZoneID == Zone.Hossin);
                int amerishCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.ZoneID == Zone.Amerish);
                int otherCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.ZoneID == 0);

                _Logger.LogInformation($"Characters being tracked:\n"
                    + $"Total: {totalCount}\n"
                    + $"VS: {vsCount}/{vsCountWorld}\n"
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

        public void Online(short worldID) {
            lock (CharacterStore.Get().Players) {
                int totalCount = CharacterStore.Get().Players.Count;

                int Count(Func<KeyValuePair<string, TrackedPlayer>, bool> predicate) {
                    return CharacterStore.Get().Players.Where(predicate).Count();
                }

                int vsCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.TeamID == Faction.VS);
                int ncCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.TeamID == Faction.NC);
                int trCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.TeamID == Faction.TR);
                int nsCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.TeamID == Faction.NS);

                int indarCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.ZoneID == Zone.Indar);
                int esamirCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.ZoneID == Zone.Esamir);
                int hossinCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.ZoneID == Zone.Hossin);
                int amerishCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.ZoneID == Zone.Amerish);
                int otherCount = Count(iter => iter.Value.WorldID == worldID && iter.Value.Online == true && iter.Value.ZoneID == 0);

                _Logger.LogInformation($"Characters being tracked (only online):\n"
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

        public void Nso() {
            lock (CharacterStore.Get().Players) {
                IEnumerable<KeyValuePair<string, TrackedPlayer>> robots = CharacterStore.Get().Players.Where(iter => iter.Value.FactionID == 4);

                int vsCount = robots.Where(iter => iter.Value.TeamID == 1).Count();
                int ncCount = robots.Where(iter => iter.Value.TeamID == 2).Count();
                int trCount = robots.Where(iter => iter.Value.TeamID == 3).Count();
                int nsCount = robots.Where(iter => iter.Value.TeamID == 4).Count();
                int otherCount = robots.Where(iter => iter.Value.TeamID != 1 && iter.Value.TeamID != 2 && iter.Value.TeamID != 3 && iter.Value.TeamID != 4).Count();

                _Logger.LogInformation($"NSO faction placement:\n"
                    + $"Total: {robots.Count()}\n"
                    + $"VS: {vsCount}\n"
                    + $"NC: {ncCount}\n"
                    + $"TR: {trCount}\n"
                    + $"NS: {nsCount}\n"
                    + $"Other: {otherCount}"
                );

            }
        }

    }
}
