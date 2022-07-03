using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Commands;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Commands {

    [Command]
    public class AlertCommand {

        private readonly ILogger<AlertCommand> _Logger;
        private readonly AlertRepository _AlertRepository;
        private readonly AlertPlayerDataRepository _DataRepository;
        private readonly AlertPlayerDataDbStore _DataDb;
        private readonly AlertPlayerProfileDataDbStore _ProfileDataDb;
        private readonly AlertPopulationDbStore _PopulationDb;
        private readonly AlertPopulationRepository _PopulationRepository;

        public AlertCommand(IServiceProvider services) {
            _Logger = services.GetRequiredService<ILogger<AlertCommand>>();
            _AlertRepository = services.GetRequiredService<AlertRepository>();
            _DataRepository = services.GetRequiredService<AlertPlayerDataRepository>();
            _DataDb = services.GetRequiredService<AlertPlayerDataDbStore>();
            _ProfileDataDb = services.GetRequiredService<AlertPlayerProfileDataDbStore>();
            _PopulationDb = services.GetRequiredService<AlertPopulationDbStore>();
            _PopulationRepository = services.GetRequiredService<AlertPopulationRepository>();
        }

        public async Task Rebuild(long alertID) {
            PsAlert? alert = await _AlertRepository.GetByID(alertID);
            if (alert == null) {
                _Logger.LogWarning($"Alert {alertID} doesn't exist");
                return;
            }

            _Logger.LogInformation($"Rebuilding participant data for alert {alertID}...");

            await _ProfileDataDb.DeleteByAlertID(alertID);
            await _DataDb.DeleteByAlertID(alertID);

            List<AlertPlayerDataEntry> parts = await _DataRepository.GetByAlert(alertID, CancellationToken.None);

            alert.Participants = parts.Count;
            await _AlertRepository.UpdateByID(alertID, alert);

            _Logger.LogInformation($"Done rebuilding participant data for alert {alertID}");
        }

        public async Task RebuildPop(long alertID) {
            PsAlert? alert = await _AlertRepository.GetByID(alertID);
            if (alert == null) {
                _Logger.LogWarning($"Alert {alertID} doesn't exist");
            }

            _Logger.LogInformation($"Rebuilding population data for alert {alertID}...");

            await _PopulationDb.DeleteByAlertID(alertID);

            List<AlertPopulation> pop = await _PopulationRepository.GetByAlertID(alertID, CancellationToken.None);

            _Logger.LogInformation($"Done rebuilding population data for alert {alertID}, have {pop.Count} entries");
        }

        public async Task Create(string name, string start, int duration, short worldID, uint zoneID) {
            if (DateTime.TryParse(start, out DateTime startDate) == false) {
                _Logger.LogWarning($"Failed to parse {start} to a valid DateTime");
            }

            PsAlert alert = new PsAlert();
            alert.Name = name;
            alert.Timestamp = startDate;
            alert.WorldID = worldID;
            alert.ZoneID = zoneID;
            alert.Duration = duration;

            alert.ID = await _AlertRepository.Insert(alert);

            _Logger.LogInformation($"Created alert {alert.ID}/{alert.Name}");
        }

    }
}
