using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Commands {

    [Command]
    public class OutfitCommand {

        private readonly ILogger<OutfitCommand> _Logger;
        private readonly IOutfitCollection _OutfitCollection;
        private readonly IOutfitDbStore _OutfitDb;

        public OutfitCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<OutfitCommand>>();
            _OutfitCollection = services.GetRequiredService<IOutfitCollection>();
            _OutfitDb = services.GetRequiredService<IOutfitDbStore>();
        }

        public async Task Refresh(string tag) {
            _Logger.LogInformation($"Refreshing info for [{tag}] from census");

            PsOutfit? outfit = await _OutfitCollection.GetByTag(tag);

            if (outfit == null) {
                _Logger.LogWarning($"Failed to find outfit [{tag}]");
                return;
            }

            PsOutfit? dbOutfit = await _OutfitDb.GetByID(outfit.ID);

            _Logger.LogInformation($"'{dbOutfit?.Name}' last updated on {dbOutfit?.LastUpdated}");

            await _OutfitDb.Upsert(outfit);
        }

    }
}
