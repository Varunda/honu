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
        private readonly OutfitCollection _OutfitCollection;
        private readonly OutfitDbStore _OutfitDb;

        public OutfitCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<OutfitCommand>>();
            _OutfitCollection = services.GetRequiredService<OutfitCollection>();
            _OutfitDb = services.GetRequiredService<OutfitDbStore>();
        }

        public async Task Refresh(string id) {
            _Logger.LogInformation($"Refreshing info for outfit {id} from census");

            PsOutfit? outfit = await _OutfitCollection.GetByID(id);

            if (outfit == null) {
                _Logger.LogInformation($"cannot refresh outfit {id}, outfit does not exist in census");
                return;
            }

            PsOutfit? dbOutfit = await _OutfitDb.GetByID(outfit.ID);

            _Logger.LogInformation($"'{dbOutfit?.Name}' last updated on {dbOutfit?.LastUpdated}");

            await _OutfitDb.Upsert(outfit);
        }

    }
}
