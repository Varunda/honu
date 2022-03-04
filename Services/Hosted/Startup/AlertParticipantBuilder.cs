using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted.Startup {

    public class AlertParticipantBuilder : BackgroundService {

        private readonly ILogger<AlertParticipantBuilder> _Logger;

        private readonly AlertParticipantDataRepository _DataRepository;
        private readonly AlertDbStore _AlertDb;

        public AlertParticipantBuilder(ILogger<AlertParticipantBuilder> logger,
            AlertParticipantDataRepository dataRepository, AlertDbStore alertDb) {

            _Logger = logger;
            _DataRepository = dataRepository;
            _AlertDb = alertDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            List<PsAlert> alerts = await _AlertDb.GetAll();
            alerts.Reverse();

            _Logger.LogDebug($"Loaded {alerts.Count} alerts");

            foreach (PsAlert alert in alerts) {
                if (alert.Participants > 0) {
                    continue;
                }

                Stopwatch timer = Stopwatch.StartNew();
                List<AlertParticipantDataEntry> existingData = await _DataRepository.GetByAlert(alert, stoppingToken);

                alert.Participants = existingData.Count;
                await _AlertDb.UpdateByID(alert.ID, alert);

                _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to load alert {alert.ID}");
            }
        }

    }
}
