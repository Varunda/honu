using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Repositories;

namespace watchtower.Commands {

    [Command]
    public class ItemCommand {

        private readonly ILogger<ItemCommand> _Logger;

        private readonly IItemRepository _ItemRepository;

        public ItemCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ItemCommand>>();
            _ItemRepository = services.GetRequiredService<IItemRepository>();
        }

        public async Task Get(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            if (item != null) {
                _Logger.LogInformation($"{JToken.FromObject(item)}");
            } else {
                _Logger.LogWarning($"Failed to find item '{itemID}'");
            }
        }

    }
}
