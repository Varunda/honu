using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
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
            CharacterFriendDbStore friendDb) {

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
            await Task.WhenAll(
                // Update the characters weapon stats
                _WeaponCensus.GetByCharacterID(charID).ContinueWith(result => weaponStats = result.Result),

                // Update the characters history stats
                _HistoryCensus.GetByCharacterID(charID).ContinueWith(result => historyStats = result.Result),

                // Update the items the character has
                _ItemCensus.GetByID(charID).ContinueWith(result => itemStats = result.Result),

                // Get the character stats (not the history ones)
                _StatCensus.GetByID(charID).ContinueWith(result => statEntries = result.Result),

                // Get the character's friends
                _FriendCensus.GetByCharacterID(charID).ContinueWith(result => charFriends = result.Result),

                // Get the character's directive data
                _CharacterDirectiveCensus.GetByCharacterID(charID).ContinueWith(result => charDirs = result.Result),
                _CharacterDirectiveTreeCensus.GetByCharacterID(charID).ContinueWith(result => charTreeDirs = result.Result),
                _CharacterDirectiveTierCensus.GetByCharacterID(charID).ContinueWith(result => charTierDirs = result.Result),
                _CharacterDirectiveObjectiveCensus.GetByCharacterID(charID).ContinueWith(result => charObjDirs = result.Result)
            );
            censusTrace?.Stop();

            using Activity? dbTrace = HonuActivitySource.Root.StartActivity("update character - db root");

            // DB WEAPON STATS
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db weapon stats")) {
                span?.AddTag("honu.count", weaponStats.Count);
                if (weaponStats.Count > 0) {
                    try {
                        foreach (WeaponStatEntry iter in weaponStats) {
                            await _WeaponStatDb.Upsert(iter);
                        }
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"error updating character weapon stat data for {charID}");
                    }
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB HISTORY STATS
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db history stats")) {
                span?.AddTag("honu.count", historyStats.Count);
                try {
                    span?.AddTag("honu.batch", "ignored");
                    foreach (PsCharacterHistoryStat stat in historyStats) {
                        await _HistoryDb.Upsert(charID, stat.Type, stat);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating history stats for {charID}");
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB ITEM UNLOCKS
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db item unlocks")) {
                span?.AddTag("honu.count", itemStats.Count);
                try {
                    if (itemStats.Count > 0) {
                        await _ItemDb.Set(charID, itemStats);
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
                    _Logger.LogError(ex, $"error updating item unlocks for {charID}");
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB STATS
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db stats")) {
                span?.AddTag("honu.count", statEntries.Count);
                try {
                    if (statEntries.Count > 0) {
                        await _StatDb.Set(charID, statEntries);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating stats for {charID}");
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB FRIENDS
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db friends")) {
                span?.AddTag("honu.count", charFriends.Count);
                try {
                    if (charFriends.Count > 0) {
                        await _FriendDb.Set(charID, charFriends);
                    }
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"error updating friends for {charID}");
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive")) {
                span?.AddTag("honu.count", charDirs.Count);
                foreach (CharacterDirective dir in charDirs) {
                    try {
                        await _CharacterDirectiveDb.Upsert(dir);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"error updating character directive data for {charID} directive ID ${dir.DirectiveID}");
                    }
                }
                /*
                try {
                    await _CharacterDirectiveDb.UpsertMany(charID, charDirs);
                } catch (Exception ex) {
                    span?.AddExceptionEvent(ex);
                    _Logger.LogError(ex, $"Error updating character directive data for {charID}");
                }
                */
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE TREE
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tree")) {
                span?.AddTag("honu.count", charTreeDirs.Count);
                foreach (CharacterDirectiveTree tree in charTreeDirs) {
                    try {
                        await _CharacterDirectiveTreeDb.Upsert(charID, tree);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive trees for {charID}");
                    }
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE TIER
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive tier")) {
                span?.AddTag("honu.count", charTierDirs.Count);
                foreach (CharacterDirectiveTier tier in charTierDirs) {
                    try {
                        await _CharacterDirectiveTierDb.Upsert(charID, tier);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive tiers for {charID}");
                    }
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            // DB DIRECTIVE OBJECTIVE
            using (Activity? span = HonuActivitySource.Root.StartActivity("update character - db directive objective")) {
                span?.AddTag("honu.count", charObjDirs.Count);
                foreach (CharacterDirectiveObjective obj in charObjDirs) {
                    try {
                        await _CharacterDirectiveObjectiveDb.Upsert(charID, obj);
                    } catch (Exception ex) {
                        span?.AddExceptionEvent(ex);
                        _Logger.LogError(ex, $"Error upserting character directive objectives for {charID}");
                    }
                }
            }
            stoppingToken.ThrowIfCancellationRequested();

            PsCharacter? character = await _CharacterCensus.GetByID(charID, CensusEnvironment.PC);

            if (character != null) {
                await _CharacterDb.Upsert(character);
            }
        }

    }
}
