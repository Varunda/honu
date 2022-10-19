using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Commands {

    [Command]
    public class ItemCommand {

        private readonly ILogger<ItemCommand> _Logger;

        private readonly ItemRepository _ItemRepository;
        private readonly WeaponUpdateQueue _Queue;

        public ItemCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<ItemCommand>>();
            _ItemRepository = services.GetRequiredService<ItemRepository>();
            _Queue = services.GetRequiredService<WeaponUpdateQueue>();
        }

        public async Task Get(int itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            if (item != null) {
                _Logger.LogInformation($"{JToken.FromObject(item)}");
            } else {
                _Logger.LogWarning($"Failed to find item '{itemID}'");
            }
        }

        public void StatUpdate(int itemID) {
            _Queue.QueueAtFront(itemID);
            _Logger.LogInformation($"Queued {itemID} next for weapon stat update");
        }

    }
}
