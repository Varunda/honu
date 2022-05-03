using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted.Startup {

    public class AlertParticipantBuilder : BackgroundService {

        private readonly ILogger<AlertParticipantBuilder> _Logger;

        private readonly AlertPlayerDataRepository _DataRepository;
        private readonly AlertDbStore _AlertDb;

        public AlertParticipantBuilder(ILogger<AlertParticipantBuilder> logger,
            AlertPlayerDataRepository dataRepository, AlertDbStore alertDb) {

            _Logger = logger;
            _DataRepository = dataRepository;
            _AlertDb = alertDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                List<PsAlert> alerts = await _AlertDb.GetAll();
                alerts = alerts.OrderByDescending(iter => iter.ID).ToList();

                _Logger.LogDebug($"Loaded {alerts.Count} alerts");

                foreach (PsAlert alert in alerts) {

                    DateTime alertEnd = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);
                    if (DateTime.UtcNow <= alertEnd) {
                        _Logger.LogInformation($"Not generating info for {alert.ID}, finishes at {alertEnd:u}");
                        continue;
                    }

                    if (alert.Participants > 0) {
                        continue;
                    }

                    Stopwatch timer = Stopwatch.StartNew();
                    List<AlertPlayerDataEntry> existingData = await _DataRepository.GetByAlert(alert, stoppingToken);

                    alert.Participants = existingData.Count;
                    await _AlertDb.UpdateByID(alert.ID, alert);

                    _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms to load alert {alert.ID} on {alert.WorldID} in zone {alert.ZoneID}");
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"error in alert participation builder");
            }
        }

    }
}
