using Microsoft.AspNetCore.SignalR;
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
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Wrapped;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedWrappedGenerationProcess : BackgroundService {

        private const string SERVICE_NAME = "wrapped_generation";

        private readonly ILogger<HostedWrappedGenerationProcess> _Logger;
        private readonly WrappedGenerationQueue _Queue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly WrappedHub _Hub;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly AchievementRepository _AchievementRepository;
        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;

        public HostedWrappedGenerationProcess(ILogger<HostedWrappedGenerationProcess> logger,
            WrappedGenerationQueue queue, IServiceHealthMonitor serviceHealthMonitor,
            WrappedHub hub, CharacterRepository characterRepository,
            OutfitRepository outfitRepository, ItemRepository itemRepository,
            AchievementRepository achievementRepository, KillEventDbStore killDb,
            ExpEventDbStore expDb) {

            _Logger = logger;
            _Queue = queue;
            _ServiceHealthMonitor = serviceHealthMonitor;
            _Hub = hub;

            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
            _ItemRepository = itemRepository;
            _AchievementRepository = achievementRepository;
            _KillDb = killDb;
            _ExpDb = expDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"started");

            while (stoppingToken.IsCancellationRequested == false) {
                WrappedEntry? entry = null;

                try {
                    ServiceHealthEntry serviceHealthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new() { Name = SERVICE_NAME };

                    if (serviceHealthEntry.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    entry = await _Queue.Dequeue(stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to fetch queue entry");
                }

                if (entry == null) {
                    continue;
                }

                try {
                    _Logger.LogDebug($"Starting entry {entry.ID} with {entry.InputCharacterIDs.Count} input characters: [{string.Join(", ", entry.InputCharacterIDs)}]");

                    Stopwatch timer = Stopwatch.StartNew();
                    await ProcessEntry(entry);
                    _Logger.LogInformation($"Processed wrapped entry {entry.ID} in {timer.ElapsedMilliseconds}ms");
                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                } catch (Exception ex) {
                    _Logger.LogDebug(ex, $"unhandled exception while processing entry {entry.ID}");
                }
            }
        }

        /// <summary>
        ///     process a single wrapped entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        protected async Task ProcessEntry(WrappedEntry entry) {
            // STARTED
            await _Hub.UpdateStatus(entry.ID, WrappedStatus.STARTED);

            if (entry.ID == Guid.Empty) {
                _Logger.LogError($"Got an empty ID when processing entry {entry.ID}");
                return;
            }

            if (entry.InputCharacterIDs.Count == 0) {
                await _Hub.SendError(entry.ID, $"This wrapped entry had 0 characters input");
                _Logger.LogWarning($"No characters for entry {entry.ID}");
                return;
            }

            WrappedEntryIdSet setsToLoad = new();

            // LOADING_INPUT_CHARACTERS
            await _Hub.UpdateStatus(entry.ID, WrappedStatus.LOADING_INPUT_CHARACTERS);
            List<PsCharacter> inputCharacters = await _CharacterRepository.GetByIDs(entry.InputCharacterIDs, CensusEnvironment.PC, fast: false);
            setsToLoad.Characters.AddRange(inputCharacters.Select(iter => iter.ID));
            _Logger.LogDebug($"Loaded {inputCharacters.Count} of {entry.InputCharacterIDs.Count} requested");
            AddCharactersToEntry(inputCharacters, entry);

            await _Hub.UpdateInputCharacters(entry.ID, inputCharacters);

        }

        protected async Task ProcessCharacter(WrappedEntry entry, PsCharacter character, WrappedEntryIdSet sets) {

        }

        private void AddCharacterToEntry(PsCharacter c, WrappedEntry entry) {
            if (entry.Characters.ContainsKey(c.ID) == false) {
                entry.Characters.Add(c.ID, c);
            }
        }

        private void AddCharactersToEntry(List<PsCharacter> chars, WrappedEntry entry) {
            foreach (PsCharacter c in chars) {
                AddCharacterToEntry(c, entry);
            }
        }

    }
}
