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

    public class HostedBackgroundCharacterPriorityUpdateQueue : BackgroundService {

        private const string SERVICE_NAME = "background_character_cache";

        private readonly ILogger<HostedBackgroundCharacterWeaponStatQueue> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly PriorityCharacterUpdateQueue _Queue;

        private readonly CharacterMetadataDbStore _MetadataDb;

        private readonly CharacterCollection _CharacterCensus;
        private readonly CharacterDbStore _CharacterDb;

        private readonly CharacterDataRepository _CharacterDataRepository;

        public HostedBackgroundCharacterPriorityUpdateQueue(ILogger<HostedBackgroundCharacterWeaponStatQueue> logger,
            PriorityCharacterUpdateQueue queue, CharacterMetadataDbStore metadataDb,
            CharacterCollection charColl, CharacterDbStore charDb,
            IServiceHealthMonitor serviceHealthMonitor, CharacterDataRepository characterDataRepository) {

            _Logger = logger;
            _Queue = queue;

            _MetadataDb = metadataDb;

            _CharacterCensus = charColl;
            _CharacterDb = charDb;
            _ServiceHealthMonitor = serviceHealthMonitor;
            _CharacterDataRepository = characterDataRepository;
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

                _Logger.LogDebug($"peaking next entry");
                CharacterUpdateQueueEntry entry = await _Queue.Peak(stoppingToken);
                _Logger.LogDebug($"peaked next entry {entry.CharacterID}");
                string cID1 = entry.CharacterID;
                _Logger.LogDebug($"peaking again, expect to see {cID1}");
                entry = await _Queue.Peak(stoppingToken);
                _Logger.LogDebug($"match? {cID1 == entry.CharacterID}, got {entry.CharacterID} from queue (expected {cID1})");
                timer.Restart();
                _Logger.LogDebug($"priority updating {entry.CharacterID}");

                try {
                    CharacterMetadata? metadata = await _MetadataDb.GetByCharacterID(entry.CharacterID);

                    using Activity? rootTrace = HonuActivitySource.Root.StartActivity("priority update character");
                    rootTrace?.AddTag("honu.characterID", entry.CharacterID);

                    PsCharacter? censusChar = null;
                    try {
                        censusChar = await _CharacterCensus.GetByID(entry.CharacterID, CensusEnvironment.PC);
                    } catch (CensusConnectionException ex) {
                        rootTrace?.AddExceptionEvent(ex);
                        _Logger.LogWarning($"Got timeout when loading {entry.CharacterID} from census, delaying 30 seconds, requeueing and retrying [Message='{ex.Message}']");
                        await Task.Delay(30 * 1000, stoppingToken);
                        continue;
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
                    } else { 
                        rootTrace?.AddTag("honu.result", "updated");
                        metadata.NotFoundCount = 0;
                        metadata.LastUpdated = DateTime.UtcNow;

                        await _CharacterDb.Upsert(censusChar);

                        try {
                            await _CharacterDataRepository.UpdateCharacter(entry.CharacterID, stoppingToken, batchUpdate: false);
                        } catch (AggregateException ex) when (ex.InnerException is CensusConnectionException) {
                            rootTrace?.AddExceptionEvent(ex);
                            _Logger.LogWarning($"Got timeout when getting data for {entry.CharacterID}, requeuing");
                            await Task.Delay(1000 * 15, stoppingToken);
                            continue;
                        }

                        _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                    }

                    await _MetadataDb.Upsert(entry.CharacterID, metadata);

                    rootTrace?.Stop();

                    _Logger.LogDebug($"updated {entry.CharacterID} in {timer.ElapsedMilliseconds}ms");

                    _Logger.LogDebug($"dequeue next entry, expect to see {cID1} right away");
                    CharacterUpdateQueueEntry popped = await _Queue.Dequeue(stoppingToken);
                    _Logger.LogDebug($"dequeued {popped.CharacterID}, expected {cID1}, match? {cID1 == popped.CharacterID}");

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
