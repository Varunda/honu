using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Code.Commands {

    [Command]
    public class LedgerCommand {

        private readonly ILogger<LedgerCommand> _Logger;
        private readonly FacilityPlayerControlDbStore _PlayerDb;

        public LedgerCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<LedgerCommand>>();
            _PlayerDb = services.GetRequiredService<FacilityPlayerControlDbStore>();
        }

        public async Task Players(long controlID) {
            List<PlayerControlEvent> events = await _PlayerDb.GetByEventID(controlID);

            foreach (PlayerControlEvent ev in events) {
                _Logger.LogInformation($"{ev.CharacterID}, {ev.OutfitID}, {ev.IsCapture}");
            }
        }

    }
}
