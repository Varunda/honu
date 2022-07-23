using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.CharacterStats;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Queues;
using watchtower.Services;
using watchtower.Services.Census;
using watchtower.Services.CharacterViewer;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Commands {

    [Command]
    public class CharCommand {

        private readonly ILogger<CharCommand> _Logger;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _Outfitrepository;
        private readonly SessionDbStore _SessionDb;
        private readonly ICharacterStatGeneratorStore _GeneratorStore;
        private readonly CharacterHistoryStatCollection _HistoryCollection;
        private readonly CharacterHistoryStatRepository _HistoryStatRepository;
        private readonly CharacterItemRepository _CharItemRepository;
        private readonly CharacterStatCollection _StatCollection;
        private readonly CharacterStatDbStore _StatDb;
        private readonly CharacterFriendRepository _CharFriend;
        private readonly CharacterDirectiveCollection _CharacterDirectiveCensus;
        private readonly CharacterDirectiveTreeCollection _CharacterDirectiveTreeCensus;
        private readonly CharacterDirectiveTierCollection _CharacterDirectiveTierCensus;
        private readonly CharacterDirectiveObjectiveCollection _CharacterDirectiveObjectiveCensus;
        private readonly CharacterDirectiveDbStore _CharacterDirectiveDb;
        private readonly CharacterWeaponStatCollection _WeaponCensus;
        private readonly CharacterWeaponStatDbStore _WeaponDb;

        private readonly CharacterUpdateQueue _Queue;

        public CharCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<CharCommand>>();

            _CharacterRepository = services.GetRequiredService<CharacterRepository>();
            _Outfitrepository = services.GetRequiredService<OutfitRepository>();
            _SessionDb = services.GetRequiredService<SessionDbStore>();
            _GeneratorStore = services.GetRequiredService<ICharacterStatGeneratorStore>();
            _HistoryCollection = services.GetRequiredService<CharacterHistoryStatCollection>();
            _HistoryStatRepository = services.GetRequiredService<CharacterHistoryStatRepository>();
            _CharItemRepository = services.GetRequiredService<CharacterItemRepository>();
            _StatCollection = services.GetRequiredService<CharacterStatCollection>();
            _StatDb = services.GetRequiredService<CharacterStatDbStore>();
            _CharFriend = services.GetRequiredService<CharacterFriendRepository>();
            _CharacterDirectiveCensus = services.GetRequiredService<CharacterDirectiveCollection>();
            _CharacterDirectiveTreeCensus = services.GetRequiredService<CharacterDirectiveTreeCollection>();
            _CharacterDirectiveTierCensus = services.GetRequiredService<CharacterDirectiveTierCollection>();
            _CharacterDirectiveObjectiveCensus = services.GetRequiredService<CharacterDirectiveObjectiveCollection>();
            _Queue = services.GetRequiredService<CharacterUpdateQueue>();
            _CharacterDirectiveDb = services.GetRequiredService<CharacterDirectiveDbStore>();
            _WeaponCensus = services.GetRequiredService<CharacterWeaponStatCollection>();
            _WeaponDb = services.GetRequiredService<CharacterWeaponStatDbStore>();
        }

        public async Task Refresh(string nameOrId) {
            if (nameOrId.Length != 19) {
                PsCharacter? c = await _CharacterRepository.GetFirstByName(nameOrId);
                if (c == null) {
                    _Logger.LogWarning($"Failed to get character by name {nameOrId}");
                    return;
                }
                nameOrId = c.ID;
            }

            CharacterUpdateQueueEntry entry = new() {
                CensusCharacter = null,
                CharacterID = nameOrId,
                Force = true,
                Print = true
            };

            _Queue.QueueAtFront(entry);

            _Logger.LogInformation($"Inserted {nameOrId} into character update queue");
        }

        public async Task Search(string name) {
            List<PsCharacter> all = await _CharacterRepository.SearchByName(name);

            string s = $"Search results: '{name}' =>\n";
            foreach (PsCharacter c in all) {
                s += $"{c.GetDisplayName()} / {c.Prestige}~{c.BattleRank} / {c.DateLastLogin:u}\n";
            }

            _Logger.LogInformation(s);
        }

        public async Task Get(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c != null) {
                _Logger.LogInformation($"{name} => {JToken.FromObject(c)}");

                if (c.OutfitID != null) {
                    PsOutfit? outfit = await _Outfitrepository.GetByID(c.OutfitID);

                    if (outfit != null) {
                        _Logger.LogInformation($"{JToken.FromObject(outfit)}");
                    }
                }
            } else {
                _Logger.LogInformation($"{name} => null");
            }
        }

        public async Task Extra(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<ExtraStatSet> sets = await _GeneratorStore.GenerateAll(c.ID);
            foreach (ExtraStatSet set in sets) {
                _Logger.LogInformation($"{set.Name}:");
                foreach (CharacterStatBase stat in set.Stats) {
                    _Logger.LogInformation($"\t{stat.Name} => {stat.Value}");
                }
            }
        }

        public async Task History(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<PsCharacterHistoryStat> stats = await _HistoryStatRepository.GetByCharacterID(c.ID);
            foreach (PsCharacterHistoryStat stat in stats) {
                _Logger.LogInformation($"{stat.Type} => {stat.AllTime}");
            }
        }

        public async Task Items(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<CharacterItem> items = await _CharItemRepository.GetByID(c.ID);

            foreach (CharacterItem item in items) {
                _Logger.LogInformation($"{item.ItemID} ({item.AccountLevel}):: {item.StackCount}");
            }
        }

        public async Task Stats(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<PsCharacterStat> stats = await _StatCollection.GetByID(c.ID);
            if (stats.Count > 0) {
                await _StatDb.Set(c.ID, stats);
            }

            foreach (PsCharacterStat stat in stats) {
                _Logger.LogInformation($"{JToken.FromObject(stat)}");
            }
        }

        public async Task Friends(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<CharacterFriend> friends = await _CharFriend.GetByCharacterID(c.ID);
            _Logger.LogInformation($"{c.Name}/{c.ID} has {friends.Count} friends");

            foreach (CharacterFriend friend in friends) {
                _Logger.LogInformation($"{friend.FriendID}");
            }
        }

        public async Task Dirs(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<CharacterDirective> dirs = await _CharacterDirectiveCensus.GetByCharacterID(c.ID);
            List<CharacterDirectiveTree> trees = await _CharacterDirectiveTreeCensus.GetByCharacterID(c.ID);
            List<CharacterDirectiveTier> tiers = await _CharacterDirectiveTierCensus.GetByCharacterID(c.ID);
            List<CharacterDirectiveObjective> objs = await _CharacterDirectiveObjectiveCensus.GetByCharacterID(c.ID);

            string s = $"{c.Name} has {dirs.Count} entries, and {dirs.Where(iter => iter.CompletionDate != null).Count()} done:\n";

            _Logger.LogInformation(s);
        }

        public async Task DirUpdate(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<CharacterDirective> dirs = await _CharacterDirectiveCensus.GetByCharacterID(c.ID);
            await _CharacterDirectiveDb.UpsertMany(c.ID, dirs);

            _Logger.LogInformation($"done!");
        }

        public async Task ItemUpdate(string name) {
            PsCharacter? c = await _CharacterRepository.GetFirstByName(name);
            if (c == null) {
                _Logger.LogWarning($"Character {name} does not exist");
                return;
            }

            List<WeaponStatEntry> entries = await _WeaponCensus.GetByCharacterID(c.ID);
            await _WeaponDb.UpsertMany(c.ID, entries);

            _Logger.LogInformation($"done!");
        }

    }
}
