using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
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
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Models.Queues;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundCharacterWeaponStatQueue : BackgroundService {

        private const string SERVICE_NAME = "background_character_cache";

        private readonly ILogger<HostedBackgroundCharacterWeaponStatQueue> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly CharacterUpdateQueue _Queue;

        private readonly CharacterMetadataDbStore _MetadataDb;

        private readonly CharacterCollection _CharacterCensus;
        private readonly CharacterDbStore _CharacterDb;
        private readonly CharacterWeaponStatCollection _WeaponCensus;
        private readonly CharacterWeaponStatDbStore _WeaponStatDb;
        private readonly CharacterHistoryStatCollection _HistoryCensus;
        private readonly CharacterHistoryStatDbStore _HistoryDb;
        private readonly CharacterItemCollection _ItemCensus;
        private readonly CharacterItemDbStore _ItemDb;
        private readonly CharacterStatCollection _StatCensus;
        private readonly CharacterStatDbStore _StatDb;
        private readonly CharacterFriendCollection _FriendCensus;
        private readonly CharacterFriendDbStore _FriendDb;
        private readonly CharacterDirectiveCollection _CharacterDirectiveCensus;
        private readonly CharacterDirectiveDbStore _CharacterDirectiveDb;
        private readonly CharacterDirectiveTreeCollection _CharacterDirectiveTreeCensus;
        private readonly CharacterDirectiveTreeDbStore _CharacterDirectiveTreeDb;
        private readonly CharacterDirectiveTierCollection _CharacterDirectiveTierCensus;
        private readonly CharacterDirectiveTierDbStore _CharacterDirectiveTierDb;
        private readonly CharacterDirectiveObjectiveCollection _CharacterDirectiveObjectiveCensus;
        private readonly CharacterDirectiveObjectiveDbStore _CharacterDirectiveObjectiveDb;

        private static int _Count = 0;

        private readonly Random _Random = new Random();

        // always print out debug for these characters
        private List<string> _Peepers = new List<string>() {
            "5429119940672421393", // ADMxVarundaVS
            "5428345446430485649" // varunda
        };

        public HostedBackgroundCharacterWeaponStatQueue(ILogger<HostedBackgroundCharacterWeaponStatQueue> logger,
            CharacterUpdateQueue queue,
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
            CharacterFriendDbStore friendDb, IServiceHealthMonitor serviceHealthMonitor) {

            _Logger = logger;
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _MetadataDb = metadataDb ?? throw new ArgumentNullException(nameof(metadataDb));

            _CharacterCensus = charColl;
            _CharacterDb = charDb;
            _WeaponStatDb = db ?? throw new ArgumentNullException(nameof(db));
            _WeaponCensus = weaponColl ?? throw new ArgumentNullException(nameof(weaponColl));
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
            _ServiceHealthMonitor = serviceHealthMonitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            int errorCount = 0;
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            Stopwatch timer = Stopwatch.StartNew();

            while (stoppingToken.IsCancellationRequested == false) {

                ServiceHealthEntry health = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new ServiceHealthEntry() { Name = SERVICE_NAME };
                if (health.Enabled == false) {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                timer.Restart();
                CharacterUpdateQueueEntry entry = await _Queue.Dequeue(stoppingToken);

                try {
                    PsCharacter? censusChar = entry.CensusCharacter;
                    CharacterMetadata? metadata = await _MetadataDb.GetByCharacterID(entry.CharacterID);

                    using Activity? rootTrace = HonuActivitySource.Root.StartActivity("update character");
                    rootTrace?.AddTag("honu.characterID", entry.CharacterID);

                    int randomValue = _Random.Next(1001);

                    entry.Print = randomValue >= 995;
                    bool batchDbUpdate = randomValue % 2 == 0;

                    rootTrace?.AddTag("honu.batch-db-update", batchDbUpdate);

                    if (censusChar == null) {
                        try {
                            censusChar = await _CharacterCensus.GetByID(entry.CharacterID, CensusEnvironment.PC);
                        } catch (CensusConnectionException ex) {
                            rootTrace?.AddExceptionEvent(ex);
                            _Logger.LogWarning($"Got timeout when loading {entry.CharacterID} from census, delaying 30 seconds, requeueing and retrying [Message='{ex.Message}']");
                            await Task.Delay(30 * 1000, stoppingToken);
                            _Queue.Queue(entry);
                            continue;
                        }
                    }

                    if (metadata == null) {
                        metadata = new CharacterMetadata() {
                            ID = entry.CharacterID,
                            LastUpdated = DateTime.MinValue
                        };
                    }

                    stoppingToken.ThrowIfCancellationRequested();

                    // 3 conditions to check:
                    //      1. The character was not found in census. This could be from a deleted character, so increment the not found count
                    //      2. The character was found in census, but the metadata is AFTER the last time the character logged in,
                    //              meaning there is no way the character could have stats that need to be updated
                    //      3. The character was found in census, and the character is logged in since the last time stats were updated
                    if (censusChar == null) {
                        ++metadata.NotFoundCount;
                        rootTrace?.AddTag("honu.result", "not found");
                    } else if (censusChar.DateLastLogin < metadata.LastUpdated && entry.Force == false) {
                        if (_Peepers.Contains(entry.CharacterID) || entry.Print == true) {
                            _Logger.LogTrace($"{entry.CharacterID}/{censusChar.Name} last login: {censusChar.DateLastLogin:u}, last update: {metadata.LastUpdated:u} ({metadata.LastUpdated - censusChar.DateLastLogin}), skipping update");
                        }
                        metadata.NotFoundCount = 0;
                        rootTrace?.AddTag("honu.result", "not update needed");
                    } else if (censusChar.DateLastLogin >= metadata.LastUpdated || entry.Force == true) {
                        if (_Peepers.Contains(entry.CharacterID) || entry.Print == true) {
                            _Logger.LogTrace($"{entry.CharacterID}/{censusChar.Name} last login: {censusChar.DateLastLogin:u}, last update: {metadata.LastUpdated:u} ({metadata.LastUpdated - censusChar.DateLastLogin}), PERFORMING UPDATE");
                        }
                        rootTrace?.AddTag("honu.result", "updated");
                        metadata.NotFoundCount = 0;
                        metadata.LastUpdated = DateTime.UtcNow;

                        List<WeaponStatEntry> weaponStats = new();
                        List<PsCharacterHistoryStat> historyStats = new();
                        List<CharacterItem> itemStats = new();
                        List<PsCharacterStat> statEntries = new();
                        List<CharacterFriend> charFriends = new();
                        List<CharacterDirective> charDirs = new();
                        List<CharacterDirectiveTree> charTreeDirs = new();
                        List<CharacterDirectiveTier> charTierDirs = new();
                        List<CharacterDirectiveObjective> charObjDirs = new();

                        await _CharacterDb.Upsert(censusChar);

                        using Activity? censusTrace = HonuActivitySource.Root.StartActivity("update character - census root");
                        try {
                            await Task.WhenAll(
                                // Update the characters weapon stats
                                _WeaponCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => weaponStats = result.Result),

                                // Update the characters history stats
                                _HistoryCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => historyStats = result.Result),

                                // Update the items the character has
                                _ItemCensus.GetByID(entry.CharacterID).ContinueWith(result => itemStats = result.Result),

                                // Get the character stats (not the history ones)
                                _StatCensus.GetByID(entry.CharacterID).ContinueWith(result => statEntries = result.Result),

                                // Get the character's friends
                                _FriendCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => charFriends = result.Result),

                                // Get the character's directive data
                                _CharacterDirectiveCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => charDirs = result.Result),
                                _CharacterDirectiveTreeCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => charTreeDirs = result.Result),
                                _CharacterDirectiveTierCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => charTierDirs = result.Result),
                                _CharacterDirectiveObjectiveCensus.GetByCharacterID(entry.CharacterID).ContinueWith(result => charObjDirs = result.Result)
                            );
                        } catch (AggregateException ex) when (ex.InnerException is CensusConnectionException) {
                            rootTrace?.AddExceptionEvent(ex);
                            _Logger.LogWarning($"Got timeout when getting data for {entry.CharacterID}, requeuing");
                            _Queue.Queue(entry);
                            await Task.Delay(1000 * 15, stoppingToken);
                            continue;
                        }
                        censusTrace?.Stop();

                        long censusTime = timer.ElapsedMilliseconds;
                        timer.Restart();

                        if (entry.Print == true) {
                            _Logger.LogDebug($"{entry.CharacterID}> Took {censusTime}ms to get data from Census.\n"
                                + $"\tWeapons: {weaponStats.Count}\n"
                                + $"\tHistory stats: {historyStats.Count}\n"
                                + $"\tItems: {itemStats.Count}\n"
                                + $"\tStat entries: {statEntries.Count}\n"
                                + $"\tFriends: {charFriends.Count}\n"
                                + $"\tDirectives: {charDirs.Count}\n"
                                + $"\tDirective trees: {charTreeDirs.Count}\n"
                                + $"\tDirective tiers: {charTierDirs.Count}\n"
                                + $"\tDirective objectives: {charObjDirs.Count}\n"
                            );
                        }

                        using Activity? dbTrace = HonuActivitySource.Root.StartActivity("update character - db root");

                        // DB WEAPON STATS
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db weapon stats")) {
                            span?.AddTag("honu.count", weaponStats.Count);
                            if (weaponStats.Count > 0) {
                                try {
                                    if (batchDbUpdate == true) {
                                        await _WeaponStatDb.UpsertMany(entry.CharacterID, weaponStats);
                                    } else {
                                        foreach (WeaponStatEntry iter in weaponStats) {
                                            await _WeaponStatDb.Upsert(iter);
                                        }
                                    }
                                } catch (Exception ex) {
                                    span?.AddExceptionEvent(ex);
                                    _Logger.LogError(ex, $"error updating character weapon stat data for {entry.CharacterID}/{entry.CensusCharacter?.Name}");
                                }
                            }
                        }
                        stoppingToken.ThrowIfCancellationRequested();
                        long dbWeapon = timer.ElapsedMilliseconds; timer.Restart();

                        // DB HISTORY STATS
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db history stats")) {
                            span?.AddTag("honu.count", historyStats.Count);
                            try {
                                span?.AddTag("honu.batch", "ignored");
                                foreach (PsCharacterHistoryStat stat in historyStats) {
                                    await _HistoryDb.Upsert(entry.CharacterID, stat.Type, stat);
                                }
                            } catch (Exception ex) {
                                span?.AddExceptionEvent(ex);
                                _Logger.LogError(ex, $"error updating history stats for {entry.CharacterID}/{entry.CensusCharacter?.Name}");
                            }
                        }
                        long dbHistory = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB ITEM UNLOCKS
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db item unlocks")) {
                            span?.AddTag("honu.count", itemStats.Count);
                            try {
                                if (itemStats.Count > 0) {
                                    await _ItemDb.Set(entry.CharacterID, itemStats);
                                }
                                /*
                                if (batchDbUpdate == true) {
                                    if (itemStats.Count > 0) {
                                        await _ItemDb.Set(entry.CharacterID, itemStats);
                                    }
                                } else {
                                    foreach (CharacterItem iter in itemStats) {
                                        await _ItemDb.Upsert(iter);
                                    }
                                }
                                */
                            } catch (Exception ex) {
                                span?.AddExceptionEvent(ex);
                                _Logger.LogError(ex, $"error updating item stats for {entry.CharacterID}/{entry.CensusCharacter?.Name}");
                            }
                        }
                        long dbItem = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB STATS
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db stats")) {
                            span?.AddTag("honu.count", statEntries.Count);
                            try {
                                if (statEntries.Count > 0) {
                                    await _StatDb.Set(entry.CharacterID, statEntries);
                                }
                            } catch (Exception ex) {
                                span?.AddExceptionEvent(ex);
                                _Logger.LogError(ex, $"error updating stats for {entry.CharacterID}/{entry.CensusCharacter?.Name}");
                            }
                        }
                        long dbStats = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB FRIENDS
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db friends")) {
                            span?.AddTag("honu.count", charFriends.Count);
                            try {
                                if (charFriends.Count > 0) {
                                    await _FriendDb.Set(entry.CharacterID, charFriends);
                                }
                            } catch (Exception ex) {
                                span?.AddExceptionEvent(ex);
                                _Logger.LogError(ex, $"error updating friends for {entry.CharacterID}/{entry.CensusCharacter?.Name}");
                            }
                        }
                        long dbFriends = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB DIRECTIVE
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive")) {
                            span?.AddTag("honu.count", charDirs.Count);
                            try {
                                await _CharacterDirectiveDb.UpsertMany(entry.CharacterID, charDirs);
                            } catch (Exception ex) {
                                span?.AddExceptionEvent(ex);
                                _Logger.LogError(ex, $"Error updating character directive data for {entry.CharacterID}");
                            }
                        }
                        long dbCharDir = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB DIRECTIVE TREE
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tree")) {
                            span?.AddTag("honu.count", charTreeDirs.Count);
                            foreach (CharacterDirectiveTree tree in charTreeDirs) {
                                try {
                                    await _CharacterDirectiveTreeDb.Upsert(entry.CharacterID, tree);
                                } catch (Exception ex) {
                                    span?.AddExceptionEvent(ex);
                                    _Logger.LogError(ex, $"Error upserting character directive trees for {entry.CharacterID}");
                                }
                            }
                        }
                        long dbCharDirTree = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB DIRECTIVE TIER
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tier")) {
                            span?.AddTag("honu.count", charTierDirs.Count);
                            foreach (CharacterDirectiveTier tier in charTierDirs) {
                                try {
                                    await _CharacterDirectiveTierDb.Upsert(entry.CharacterID, tier);
                                } catch (Exception ex) {
                                    span?.AddExceptionEvent(ex);
                                    _Logger.LogError(ex, $"Error upserting character directive tiers for {entry.CharacterID}");
                                }
                            }
                        }
                        long dbCharDirTier = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        // DB DIRECTIVE OBJECTIVE
                        using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive objective")) {
                            span?.AddTag("honu.count", charObjDirs.Count);
                            foreach (CharacterDirectiveObjective obj in charObjDirs) {
                                try {
                                    await _CharacterDirectiveObjectiveDb.Upsert(entry.CharacterID, obj);
                                } catch (Exception ex) {
                                    span?.AddExceptionEvent(ex);
                                    _Logger.LogError(ex, $"Error upserting character directive objectives for {entry.CharacterID}");
                                }
                            }
                        }
                        long dbCharDirObj = timer.ElapsedMilliseconds; timer.Restart();
                        stoppingToken.ThrowIfCancellationRequested();

                        long dbSum = dbWeapon + dbHistory + dbItem + dbStats + dbFriends + dbCharDir + dbCharDirTree + dbCharDirTier + dbCharDirObj;

                        if (entry.Print == true) {
                            _Logger.LogDebug($"{entry.CharacterID}/{censusChar.Name}> Took {dbSum}ms to update\n"
                                + $"\tWeapon: {dbWeapon}ms\n"
                                + $"\tHistory stats: {dbHistory}ms\n"
                                + $"\tItem unlocks: {dbItem}ms\n"
                                + $"\tStat entries: {dbStats}ms\n"
                                + $"\tFriends: {dbFriends}ms\n"
                                + $"\tDirectives: {dbCharDir}ms\n"
                                + $"\tDirective trees: {dbCharDirTree}ms\n"
                                + $"\tDirective tiers: {dbCharDirTier}ms\n"
                                + $"\tDirective objs: {dbCharDirObj}ms"
                            );

                            _Logger.LogDebug($"Took {censusTime}ms to get data from census, {dbSum}ms to update DB data");
                        }

                        dbTrace?.Stop();

                        _Queue.AddProcessTime(censusTime + dbSum);
                    }

                    await _MetadataDb.Upsert(entry.CharacterID, metadata);

                    rootTrace?.Stop();

                    ++_Count;

                    if (_Count % 500 == 0) {
                        _Logger.LogDebug($"Cached {_Count} characters");
                    }

                    errorCount = 0;
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"Failed in {nameof(HostedBackgroundCharacterWeaponStatQueue)}");
                    ++errorCount;

                    if (errorCount > 2) {
                        await Task.Delay(1000 * Math.Min(5, errorCount), stoppingToken);
                    }
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopped {SERVICE_NAME} with {_Queue.Count()} left");
                }

                health.LastRan = DateTime.UtcNow;

                _ServiceHealthMonitor.Set(SERVICE_NAME, health);
            }
        }

    }
}
