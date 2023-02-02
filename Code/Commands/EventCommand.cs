using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using watchtower.Commands;
using watchtower.Services.Queues;

namespace watchtower.Code.Commands {

    [Command]
    public class EventCommand {

        private readonly ILogger<EventCommand> _Logger;
        private readonly CensusRealtimeEventQueue _Queue;

        public EventCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<EventCommand>>();
            _Queue = services.GetRequiredService<CensusRealtimeEventQueue>();
        }

        public void StartAlert() {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            JToken token = JToken.Parse($@"
                {{
                    'type': 'serviceMessage',
                    'payload': {{
                        'event_name': 'MetagameEvent',
                        'experience_bonus': '25.000000',
                        'faction_nc': '33.333332',
                        'faction_tr': '34.509804',
                        'faction_vs': '31.764706',
                        'instance_id': '42533',
                        'metagame_event_id': '211',
                        'metagame_event_state': '135',
                        'metagame_event_state_name': 'started',
                        'timestamp': '{now}',
                        'world_id': '17',
                        'zone_id': '6'
                    }}
                }}
            ");

            _Queue.Queue(token);
        }

        public void WorldFilter(short worldID) {
            if (worldID == 0) {
                Logging.WorldIDFilter = null;
                _Logger.LogInformation($"Cleared world ID filter");
            } else {
                Logging.WorldIDFilter = worldID;
                _Logger.LogInformation($"World ID filter set to {worldID}");
            }
        }

    }
}
