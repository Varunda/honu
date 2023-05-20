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

        private const string SERVICE_NAME = "backgroud_character_cache";

        private readonly ILogger<HostedBackgroundCharacterWeaponStatQueue> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly CharacterUpdateQueue _Queue;

        private readonly CharacterMetadataDbStore _MetadataDb;

        private readonly CharacterCollection _CharacterCensus;
        private readonly CharacterDbStore _CharacterDb;

        private readonly CharacterDataRepository _CharacterDataRepository;

        private static int _Count = 0;

        private readonly Random _Random = new Random();

        // always print out debug for these characters
        private List<string> _Peepers = new List<string>() {
            "5429119940672421393", // ADMxVarundaVS
            "5428345446430485649" // varunda
        };

        public HostedBackgroundCharacterWeaponStatQueue(ILogger<HostedBackgroundCharacterWeaponStatQueue> logger,
            CharacterUpdateQueue queue, CharacterMetadataDbStore metadataDb,
            CharacterCollection charColl, CharacterDbStore charDb,
            IServiceHealthMonitor serviceHealthMonitor, CharacterDataRepository characterDataRepository) {

            _Logger = logger;
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _MetadataDb = metadataDb ?? throw new ArgumentNullException(nameof(metadataDb));

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

                CharacterUpdateQueueEntry entry = await _Queue.Dequeue(stoppingToken);
                timer.Restart();

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

                        await _CharacterDb.Upsert(censusChar);

                        try {
                            await _CharacterDataRepository.UpdateCharacter(entry.CharacterID, stoppingToken, batchUpdate: batchDbUpdate);
                        } catch (AggregateException ex) when (ex.InnerException is CensusConnectionException) {
                            rootTrace?.AddExceptionEvent(ex);
                            _Logger.LogWarning($"Got timeout when getting data for {entry.CharacterID}, requeuing");
                            _Queue.Queue(entry);
                            await Task.Delay(1000 * 15, stoppingToken);
                            continue;
                        }

                        _Queue.AddProcessTime(timer.ElapsedMilliseconds);
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
