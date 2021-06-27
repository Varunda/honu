using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundCharacterCacheQueue : BackgroundService {

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

                    if (character != null && character.OutfitID != null) {
                        await _OutfitRepository.GetByID(character.OutfitID);
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Error while caching character");
                }
            }
        }

    }
}
