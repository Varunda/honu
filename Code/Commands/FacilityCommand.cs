using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Db;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Code.Commands {

    [Command]
    public class FacilityCommand {

        private readonly ILogger<FacilityCommand> _Logger;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly FacilityCollection _FacilityCollection;

        public FacilityCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<FacilityCommand>>();
            _ControlDb = services.GetRequiredService<FacilityControlDbStore>();
            _FacilityCollection = services.GetRequiredService<FacilityCollection>();
        }

        public async Task Control() {
            FacilityControlOptions param = new FacilityControlOptions();

            List<FacilityControlDbEntry> entries = await _ControlDb.Get(param);

            _Logger.LogInformation($"Entries {entries.Count}");
            foreach (FacilityControlDbEntry entry in entries) {
                _Logger.LogInformation($"{entry.FacilityID} C:{entry.Captured} ({entry.CaptureAverage}) ::: D:{entry.Defended} ({entry.DefenseAverage})");
            }
        }

        public async Task GetAll() {
            await _FacilityCollection.GetAll();
        }

    }
}
