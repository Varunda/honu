using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Queues;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundCharacterCacheQueue : BackgroundService {

        private const string SERVICE_NAME = "background_character_cache";

        private readonly ILogger<HostedBackgroundCharacterCacheQueue> _Logger;
        private readonly CharacterCacheQueue _Queue;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;

        public HostedBackgroundCharacterCacheQueue(ILogger<HostedBackgroundCharacterCacheQueue> logger,
            CharacterCacheQueue queue, CharacterRepository charRepo,
            OutfitRepository outfitRepo) {

            _Logger = logger;
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    CharacterFetchQueueEntry entry = await _Queue.Dequeue(stoppingToken);

                    string charID = entry.CharacterID;
                    PsCharacter? character = await _CharacterRepository.GetByID(charID);

                    if (character != null && entry.Store == true) {
                        lock (CharacterStore.Get().Players) {
                            TrackedPlayer tracked = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                                ID = charID,
                                FactionID = character.FactionID,
                                TeamID = character.FactionID,
                                Online = false,
                                WorldID = character.WorldID,
                                OutfitID = character.OutfitID
                            });

                            tracked.OutfitID = character.OutfitID;
                            tracked.FactionID = character.FactionID;

                            // Prevent the TeamID field from being overriden whenever a character is cached
                            if (tracked.FactionID != Faction.NS) {
                                tracked.TeamID = character.FactionID;
                            }
                        }
                    }

                    if (character != null && character.OutfitID != null) {
                        await _OutfitRepository.GetByID(character.OutfitID);
                    }
                } catch (CensusServiceUnavailableException) {
                    _Logger.LogWarning($"Failed to get character from API, service unavailable");
                    await Task.Delay(30 * 1000, stoppingToken);
                } catch (CensusConnectionException) {
                    _Logger.LogWarning($"Failed to get character from API, connection exception");
                    await Task.Delay(30 * 1000, stoppingToken);
                } catch (CensusException) {
                    _Logger.LogWarning($"Failed to get character from API, general exception");
                    await Task.Delay(30 * 1000, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error while caching character");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_Queue.Count()} left");
                }
            }
        }

    }
}
