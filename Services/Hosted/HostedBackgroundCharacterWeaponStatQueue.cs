using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly BackgroundCharacterWeaponStatQueue _Queue;

        private readonly CharacterMetadataDbStore _MetadataDb;

        private readonly ICharacterCollection _CharacterCensus;
        private readonly CharacterDbStore _CharacterDb;
        private readonly ICharacterWeaponStatCollection _WeaponCensus;
        private readonly ICharacterWeaponStatDbStore _WeaponStatDb;
        private readonly ICharacterHistoryStatCollection _HistoryCensus;
        private readonly ICharacterHistoryStatDbStore _HistoryDb;
        private readonly ICharacterItemCollection _ItemCensus;
        private readonly ICharacterItemDbStore _ItemDb;
        private readonly ICharacterStatCollection _StatCensus;
        private readonly ICharacterStatDbStore _StatDb;

        private static int _Count = 0;

        private List<string> _Peepers = new List<string>() {
            "5429119940672421393", "5428345446430485649"
        };

        public HostedBackgroundCharacterWeaponStatQueue(ILogger<HostedBackgroundCharacterWeaponStatQueue> logger,
            BackgroundCharacterWeaponStatQueue queue,
            ICharacterWeaponStatDbStore db, ICharacterWeaponStatCollection weaponColl,
            ICharacterHistoryStatDbStore hDb, ICharacterHistoryStatCollection hColl,
            ICharacterItemCollection itemCensus, ICharacterItemDbStore itemDb,
            ICharacterStatCollection statCensus, ICharacterStatDbStore statDb,
            CharacterMetadataDbStore metadataDb, ICharacterCollection charColl,
            CharacterDbStore charDb) {

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            int errorCount = 0;
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            Stopwatch timer = Stopwatch.StartNew();

            while (stoppingToken.IsCancellationRequested == false) {
                timer.Restart();
                CharacterUpdateQueueEntry entry = await _Queue.Dequeue(stoppingToken);

                try {
                    PsCharacter? censusChar = null;
                    CharacterMetadata? metadata = null;

                    // If the queue entry came with a character (say from the logout buffer), use that instead, saving a Census call
                    Task[] tasks = new Task[2];
                    if (entry.CensusCharacter != null) {
                        tasks[0] = Task.CompletedTask;
                        censusChar = entry.CensusCharacter;
                    } else {
                        tasks[0] = _CharacterCensus.GetByID(entry.CharacterID).ContinueWith(result => censusChar = result.Result);
                    }

                    tasks[1] = _MetadataDb.GetByCharacterID(entry.CharacterID).ContinueWith(result => metadata = result.Result);

                    await Task.WhenAll(tasks);

                    if (metadata == null) {
                        metadata = new CharacterMetadata() {
                            ID = entry.CharacterID,
                            LastUpdated = DateTime.MinValue
                        };
                    }

                    // 3 conditions to check:
                    //      1. The character was not found in census. This could be from a deleted character, so increment the not found count
                    //      2. The character was found in census, but the metadata is AFTER the last time the character logged in,
                    //              meaning there is no way the character could have stats that need to be updated
                    //      3. The character was found in census, and the character is logged in since the last time stats were updated
                    if (censusChar == null) {
                        ++metadata.NotFoundCount;
                    } else if (censusChar.DateLastLogin < metadata.LastUpdated) {
                        if (_Peepers.Contains(entry.CharacterID)) {
                            _Logger.LogTrace($"{entry.CharacterID} last login: {censusChar.DateLastLogin:u}, last update: {metadata.LastUpdated:u} ({metadata.LastUpdated - censusChar.DateLastLogin}), skipping update");
                        }
                        metadata.NotFoundCount = 0;
                    } else if (censusChar.DateLastLogin >= metadata.LastUpdated) {
                        if (_Peepers.Contains(entry.CharacterID)) {
                            _Logger.LogTrace($"{entry.CharacterID} last login: {censusChar.DateLastLogin:u}, last update: {metadata.LastUpdated:u} ({metadata.LastUpdated - censusChar.DateLastLogin}), PERFORMING UPDATE");
                        }
                        metadata.NotFoundCount = 0;
                        metadata.LastUpdated = DateTime.UtcNow;
                        await _CharacterDb.Upsert(censusChar);

                        await Task.WhenAll(
                            // Update the characters weapon stats
                            _WeaponCensus.GetByCharacterID(entry.CharacterID).ContinueWith(async result => {
                                foreach (WeaponStatEntry entry in result.Result) {
                                    await _WeaponStatDb.Upsert(entry);
                                }
                            }),

                            // Update the characters history stats
                            _HistoryCensus.GetByCharacterID(entry.CharacterID).ContinueWith(async result => {
                                foreach (PsCharacterHistoryStat stat in result.Result) {
                                    await _HistoryDb.Upsert(entry.CharacterID, stat.Type, stat);
                                }
                            }),

                            // Update the items the character has
                            _ItemCensus.GetByID(entry.CharacterID).ContinueWith(async result => {
                                // Because set will remove old entries, we don't want to accidentally
                                //      delete a perfectly good copy of the DB data if for some reason
                                //      a blank copy is returned from Census
                                if (result.Result.Count > 0) {
                                    await _ItemDb.Set(entry.CharacterID, result.Result);
                                }
                            }),

                            // Get the character stats (not the history ones)
                            _StatCensus.GetByID(entry.CharacterID).ContinueWith(async result => {
                                if (result.Result.Count > 0) {
                                    await _StatDb.Set(entry.CharacterID, result.Result);
                                }
                            })
                        );
                    }

                    await _MetadataDb.Upsert(entry.CharacterID, metadata);

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
            }
        }

    }
}
