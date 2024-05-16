using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Repositories {

    public class CharacterDataRepository {

        private readonly ILogger<CharacterDataRepository> _Logger;
        private readonly CharacterMetadataDbStore _MetadataDb;

        private readonly CharacterCollection _CharacterCensus;
        private readonly CharacterDbStore _CharacterDb;
        private readonly CharacterMetadataDbStore _CharacterMetadataDb;
        private readonly CharacterRepository _CharacterRepository;
        private readonly CharacterNameChangeDbStore _CharacterNameChangeDb;

        private readonly CharacterWeaponStatCollection _WeaponCensus;
        private readonly CharacterWeaponStatDbStore _WeaponStatDb;
        private readonly CharacterWeaponStatRepository _WeaponRepository;
        private readonly CharacterHistoryStatCollection _HistoryCensus;
        private readonly CharacterHistoryStatDbStore _HistoryDb;
        private readonly CharacterHistoryStatRepository _HistoryRepository;
        private readonly CharacterItemCollection _ItemCensus;
        private readonly CharacterItemDbStore _ItemDb;
        private readonly CharacterItemRepository _ItemRepository;
        private readonly CharacterStatCollection _StatCensus;
        private readonly CharacterStatDbStore _StatDb;
        private readonly CharacterStatRepository _StatRepository;
        private readonly CharacterFriendCollection _FriendCensus;
        private readonly CharacterFriendDbStore _FriendDb;
        private readonly CharacterFriendRepository _CharactacterFriendRepository;
        private readonly CharacterDirectiveCollection _CharacterDirectiveCensus;
        private readonly CharacterDirectiveDbStore _CharacterDirectiveDb;
        private readonly CharacterDirectiveRepository _CharacterDirectiveRepository;
        private readonly CharacterDirectiveTreeCollection _CharacterDirectiveTreeCensus;
        private readonly CharacterDirectiveTreeDbStore _CharacterDirectiveTreeDb;
        private readonly CharacterDirectiveTreeRepository _CharacterDirectiveTreeRepository;
        private readonly CharacterDirectiveTierCollection _CharacterDirectiveTierCensus;
        private readonly CharacterDirectiveTierDbStore _CharacterDirectiveTierDb;
        private readonly CharacterDirectiveTierRepository _CharacterDirectiveTierRepository;
        private readonly CharacterDirectiveObjectiveCollection _CharacterDirectiveObjectiveCensus;
        private readonly CharacterDirectiveObjectiveDbStore _CharacterDirectiveObjectiveDb;
        private readonly CharacterDirectiveObjectiveRepository _CharacterDirectiveObjectiveRepository;

        public CharacterDataRepository(ILogger<CharacterDataRepository> logger,
            CharacterWeaponStatDbStore db, CharacterWeaponStatCollection weaponColl,
            CharacterHistoryStatDbStore hDb, CharacterHistoryStatCollection hColl,
            CharacterItemCollection itemCensus, CharacterItemDbStore itemDb,
            CharacterStatCollection statCensus, CharacterStatDbStore statDb,
            CharacterMetadataDbStore metadataDb, CharacterCollection charColl,
            CharacterDbStore charDb, CharacterFriendCollection friendCensus,
            CharacterDirectiveCollection charDirCensus, CharacterDirectiveDbStore charDirDb,
            CharacterDirectiveTreeCollection charDirTreeCensus, CharacterDirectiveTreeDbStore charDirTreeDb,
            CharacterDirectiveTierCollection charDirTierCensus, CharacterDirectiveTierDbStore charDirTierDb,
            CharacterDirectiveObjectiveCollection charDirObjectiveCensus, CharacterDirectiveObjectiveDbStore charDirObjectiveDb,
            CharacterFriendDbStore friendDb, CharacterMetadataDbStore characterMetadataDb,
            CharacterRepository characterRepository,
            CharacterDirectiveObjectiveRepository characterDirectiveObjectiveRepository,
            CharacterDirectiveTierRepository characterDirectiveTierRepository,
            CharacterDirectiveTreeRepository characterDirectiveTreeRepository, CharacterDirectiveRepository characterDirectiveRepository,
            CharacterFriendRepository charactacterFriendRepository, CharacterStatRepository statRepository,
            CharacterItemRepository itemRepository, CharacterHistoryStatRepository historyRepository,
            CharacterWeaponStatRepository weaponRepository, CharacterNameChangeDbStore characterNameChangeDb) {

            _Logger = logger;

            _MetadataDb = metadataDb;

            _CharacterCensus = charColl;
            _CharacterDb = charDb;
            _WeaponStatDb = db;
            _WeaponCensus = weaponColl;
            _HistoryCensus = hColl;
            _HistoryDb = hDb;
            _ItemCensus = itemCensus;
            _ItemDb = itemDb;
            _StatCensus = statCensus;
            _StatDb = statDb;
            _FriendCensus = friendCensus;
            _FriendDb = friendDb;
            _CharacterDirectiveCensus = charDirCensus;
            _CharacterDirectiveDb = charDirDb;
            _CharacterDirectiveTreeCensus = charDirTreeCensus;
            _CharacterDirectiveTreeDb = charDirTreeDb;
            _CharacterDirectiveTierCensus = charDirTierCensus;
            _CharacterDirectiveTierDb = charDirTierDb;
            _CharacterDirectiveObjectiveCensus = charDirObjectiveCensus;
            _CharacterDirectiveObjectiveDb = charDirObjectiveDb;
            _CharacterMetadataDb = characterMetadataDb;
            _CharacterRepository = characterRepository;
            _CharacterDirectiveObjectiveRepository = characterDirectiveObjectiveRepository;
            _CharacterDirectiveTierRepository = characterDirectiveTierRepository;
            _CharacterDirectiveTreeRepository = characterDirectiveTreeRepository;
            _CharacterDirectiveRepository = characterDirectiveRepository;
            _CharactacterFriendRepository = charactacterFriendRepository;
            _StatRepository = statRepository;
            _ItemRepository = itemRepository;
            _HistoryRepository = historyRepository;
            _WeaponRepository = weaponRepository;
            _CharacterNameChangeDb = characterNameChangeDb;
        }

        public async Task UpdateCharacter(string charID, CancellationToken stoppingToken, bool batchUpdate = false) {
            List<WeaponStatEntry> weaponStats = new();
            List<PsCharacterHistoryStat> historyStats = new();
            List<CharacterItem> itemStats = new();
            List<PsCharacterStat> statEntries = new();
            List<CharacterFriend> charFriends = new();
            List<CharacterDirective> charDirs = new();
            List<CharacterDirectiveTree> charTreeDirs = new();
            List<CharacterDirectiveTier> charTierDirs = new();
            List<CharacterDirectiveObjective> charObjDirs = new();

            using Activity? censusTrace = HonuActivitySource.Root.StartActivity("update character - census root");

            // break each task into a task, so if one task fails, not all of them fail as well
            Task<List<WeaponStatEntry>> weaponTask = _WeaponCensus.GetByCharacterID(charID);
            Task<List<PsCharacterHistoryStat>> historyTask = _HistoryCensus.GetByCharacterID(charID);
            Task<List<CharacterItem>> itemTask = _ItemCensus.GetByID(charID);
            Task<List<PsCharacterStat>> statTask = _StatCensus.GetByID(charID);
            Task<List<CharacterFriend>> friendTask = _FriendCensus.GetByCharacterID(charID);
            Task<List<CharacterDirective>> charDirTask = _CharacterDirectiveCensus.GetByCharacterID(charID);
            Task<List<CharacterDirectiveTree>> charDirTreeTask = _CharacterDirectiveTreeCensus.GetByCharacterID(charID);
            Task<List<CharacterDirectiveTier>> charDirTierTask = _CharacterDirectiveTierCensus.GetByCharacterID(charID);
            Task<List<CharacterDirectiveObjective>> charDirObjTask = _CharacterDirectiveObjectiveCensus.GetByCharacterID(charID);
            Task[] tasks = [
                weaponTask,
                historyTask,
                itemTask,
                statTask,
                friendTask,
                charDirTask, charDirTreeTask, charDirTierTask, charDirObjTask
            ];

            try {
                await Task.WhenAll(tasks);
            } catch (Exception ex) {
                _Logger.LogError(ex, $"partial or complete failure to update a character [charID={charID}] [tasks={string.Join(", ", tasks.Select(iter => iter.Status))}]");
            }
            censusTrace?.Stop();

            using Activity? dbTrace = HonuActivitySource.Root.StartActivity("update character - db root");

            // DB WEAPON STATS
            if (weaponTask.IsCompletedSuccessfully == true) {
                weaponStats = weaponTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db weapon stats");
                span?.AddTag("honu.count", weaponStats.Count);
                if (weaponStats.Count > 0) {
                    try {
                        foreach (WeaponStatEntry iter in weaponStats) {
                            await _WeaponRepository.Upsert(iter);
                        }
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"error updating character weapon stat data for {charID}");
                    }
                }
            } else {
                _Logger.LogWarning($"could not update weapon stats for character, task was not completed [charID={charID}] [taskStatus={weaponTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB HISTORY STATS
            if (historyTask.IsCompletedSuccessfully == true) {
                historyStats = historyTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db history stats");
                span?.AddTag("honu.count", historyStats.Count);
                try {
                    span?.AddTag("honu.batch", "ignored");
                    foreach (PsCharacterHistoryStat stat in historyStats) {
                        await _HistoryRepository.Upsert(charID, stat.Type, stat);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating history stats for {charID}");
                }
            } else {
                _Logger.LogWarning($"could not update history stats for character, task was not completed [charID={charID}] [taskStatus={historyTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB ITEM UNLOCKS
            if (itemTask.IsCompletedSuccessfully == true) {
                itemStats = itemTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db item unlocks");
                span?.AddTag("honu.count", itemStats.Count);
                try {
                    foreach (CharacterItem iter in itemStats) {
                        await _ItemRepository.Upsert(iter);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating item unlocks for {charID}");
                }
            } else {
                _Logger.LogWarning($"could not update items for character, task was not completed [charID={charID}] [taskStatus={itemTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB STATS
            if (statTask.IsCompletedSuccessfully == true) {
                statEntries = statTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db stats");
                span?.AddTag("honu.count", statEntries.Count);
                try {
                    if (statEntries.Count > 0) {
                        await _StatRepository.Set(charID, statEntries);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating stats for {charID}");
                }
            } else {
                _Logger.LogWarning($"could not update stats for character, task was not completed [charID={charID}] [taskStatus={statTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB FRIENDS
            if (friendTask.IsCompletedSuccessfully == true) {
                charFriends = friendTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db friends");
                span?.AddTag("honu.count", charFriends.Count);
                try {
                    if (charFriends.Count > 0) {
                        await _CharactacterFriendRepository.Set(charID, charFriends);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating friends for {charID}");
                }
            } else {
                _Logger.LogWarning($"could not update friends for character, task was not completed [charID={charID}] [taskStatus={friendTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE
            if (charDirTask.IsCompletedSuccessfully == true) {
                charDirs = charDirTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive");
                span?.AddTag("honu.count", charDirs.Count);
                foreach (CharacterDirective dir in charDirs) {
                    try {
                        await _CharacterDirectiveRepository.Upsert(charID, dir);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"error updating character directive data for {charID} directive ID ${dir.DirectiveID}");
                    }
                }
            } else {
                _Logger.LogWarning($"could not update directives for character, task was not completed [charID={charID}] [taskStatus={charDirTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE TREE
            if (charDirTreeTask.IsCompletedSuccessfully == true) {
                charTreeDirs = charDirTreeTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tree");
                span?.AddTag("honu.count", charTreeDirs.Count);
                foreach (CharacterDirectiveTree tree in charTreeDirs) {
                    try {
                        await _CharacterDirectiveTreeRepository.Upsert(charID, tree);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive trees for {charID}");
                    }
                }
            } else {
                _Logger.LogWarning($"could not update directive tree for character, task was not completed [charID={charID}] [taskStatus={charDirTreeTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE TIER
            if (charDirTierTask.IsCompletedSuccessfully == true) {
                charTierDirs = charDirTierTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tier");
                span?.AddTag("honu.count", charTierDirs.Count);
                foreach (CharacterDirectiveTier tier in charTierDirs) {
                    try {
                        await _CharacterDirectiveTierRepository.Upsert(charID, tier);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive tiers for {charID}");
                    }
                }
            } else {
                _Logger.LogWarning($"could not update directive tier for character, task was not completed [charID={charID}] [taskStatus={charDirTierTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE OBJECTIVE
            if (charDirObjTask.IsCompletedSuccessfully == true) {
                charObjDirs = charDirObjTask.Result;
                using Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive objective");
                span?.AddTag("honu.count", charObjDirs.Count);
                foreach (CharacterDirectiveObjective obj in charObjDirs) {
                    try {
                        await _CharacterDirectiveObjectiveRepository.Upsert(charID, obj);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive objectives for {charID}");
                    }
                }
            } else {
                _Logger.LogWarning($"could not update directive objectives for character, task was not completed [charID={charID}] [taskStatus={charDirObjTask.Status}]");
            }
            stoppingToken.ThrowIfCancellationRequested();

            PsCharacter? character = await _CharacterCensus.GetByID(charID, CensusEnvironment.PC);
            if (character != null) {
                character.LastUpdated = DateTime.UtcNow;
                // update on the character repo so if they are cached it stays up to date
                await _CharacterRepository.Upsert(character);

                CharacterMetadata metadata = await _CharacterMetadataDb.GetByCharacterID(charID) ?? new CharacterMetadata() {
                    ID = charID,
                    LastUpdated = DateTime.UtcNow,
                    NotFoundCount = 0
                };

                // TODO 2024-05-12: do we want to update the LastUpdated even if one of the parts of the update failed?
                metadata.LastUpdated = DateTime.UtcNow;
                metadata.NotFoundCount = 0;
                await _CharacterMetadataDb.Upsert(charID, metadata);
            }

        }

    }
}
