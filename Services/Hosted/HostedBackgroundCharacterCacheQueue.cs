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
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundCharacterCacheQueue : BackgroundService {

        private const string SERVICE_NAME = "background_character_cache";

        private readonly ILogger<HostedBackgroundCharacterCacheQueue> _Logger;
        private readonly IBackgroundCharacterCacheQueue _Queue;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IOutfitRepository _OutfitRepository;

        public HostedBackgroundCharacterCacheQueue(ILogger<HostedBackgroundCharacterCacheQueue> logger,
            IBackgroundCharacterCacheQueue queue, ICharacterRepository charRepo,
            IOutfitRepository outfitRepo) {

            _Logger = logger;
            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));

            _CharacterRepository = charRepo ?? throw new ArgumentNullException(nameof(charRepo));
            _OutfitRepository = outfitRepo ?? throw new ArgumentNullException(nameof(outfitRepo));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    string charID = await _Queue.DequeueAsync(stoppingToken);

                    PsCharacter? character = await _CharacterRepository.GetByID(charID);

                    if (character != null) {
                        lock (CharacterStore.Get().Players) {
                            TrackedPlayer tracked = CharacterStore.Get().Players.GetOrAdd(charID, new TrackedPlayer() {
                                ID = charID,
                                FactionID = character.FactionID,
                                TeamID = character.FactionID,
                                Online = false,
                                WorldID = character.WorldID
                            });

                            /*
                            // Prevent the TeamID field from being overriden whenever a character is cached
                            if (tracked.FactionID != Faction.NS) {
                                tracked.TeamID = character.FactionID;
                            }

                            tracked.TeamID = character.FactionID;
                            */
                        }
                    }

                    if (character != null && character.OutfitID != null) {
                        await _OutfitRepository.GetByID(character.OutfitID);
                    }
                } catch (CensusServiceUnavailableException) {
                    _Logger.LogWarning($"Failed to get character from API");
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error while caching character");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME}");
                }
            }
        }

    }
}
