using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Queues;
using watchtower.Services.Queues;

namespace watchtower.Code.Commands {

    [Command]
    public class QueueCommand {

        private readonly ILogger<QueueCommand> _Logger;
        private readonly PriorityCharacterUpdateQueue _PriorityQueue;

        public QueueCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<QueueCommand>>();
            _PriorityQueue = services.GetRequiredService<PriorityCharacterUpdateQueue>();
        }

        public async Task ClearPrio() {
            Task task = Task.Run(async () => {
                _Logger.LogInformation($"Force clearing character priority update queue");
                while (_PriorityQueue.Count() > 0) {
                    CharacterUpdateQueueEntry c = await _PriorityQueue.Dequeue(CancellationToken.None);
                    _Logger.LogDebug($"removing {c.CharacterID} from queue");
                }
            });

            _Logger.LogInformation($"Clearning the priority queue will timeout in 30 seconds");
            await task.WaitAsync(TimeSpan.FromSeconds(30));

            _Logger.LogInformation($"Queue cleared");
        }

    }
}
