using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedDailyAlertCreator : BackgroundService {

        private readonly ILogger<HostedDailyAlertCreator> _Logger;
        private readonly IOptions<DailyAlertOptions> _Options;

        private readonly AlertDbStore _AlertDb;
        private readonly AlertPlayerDataRepository _ParticipantDataRepository;

        public HostedDailyAlertCreator(ILogger<HostedDailyAlertCreator> logger, IOptions<DailyAlertOptions> options,
            AlertDbStore alertDb, AlertPlayerDataRepository participantDataRepository) {

            _Logger = logger;
            _Options = options;
            _AlertDb = alertDb;
            _ParticipantDataRepository = participantDataRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"{JToken.FromObject( _Options.Value)}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    foreach (DailyAlertConfigEntry entry in _Options.Value.Worlds) {
                        DateTime when = DateTime.UtcNow.Date;
                        TimeSpan span = TimeSpan.Parse(entry.When);

                        DateTime w = when + span;
                        DateTime alertStart = w - TimeSpan.FromDays(1);

                        if (DateTime.UtcNow > w && (DateTime.UtcNow - TimeSpan.FromMinutes(1) <= w)) {
                            new Thread(async () => {
                                _Logger.LogDebug($"Making alert for {entry.WorldID} start at {alertStart:u} in new thread");
                                await GenerateAlert(entry.WorldID, alertStart, stoppingToken);
                            }).Start();
                        }
                    }

                    await Task.Delay(1000 * 60, stoppingToken); // 1 min
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"error generating daily alert");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"stopping");
                }
            }

        }

        private async Task GenerateAlert(short worldID, DateTime start, CancellationToken stoppingToken) {
            PsAlert alert = new();
            alert.Timestamp = start;
            alert.Duration = 60 * 60 * 24; // 60 seconds * 60 minutes * 24 hours
            alert.Name = $"{start:yyyy-MM-dd}-{World.GetName(worldID)}";
            alert.WorldID = worldID;
            alert.ZoneID = 0; // 0 means global

            alert.ID = await _AlertDb.Insert(alert);

            _Logger.LogInformation($"Creating daily alert {alert.ID}/{alert.Name} from {alert.Timestamp:u} to {(alert.Timestamp + TimeSpan.FromSeconds(alert.Duration)):u} ");

            List<AlertPlayerDataEntry> parts = await _ParticipantDataRepository.GetByAlert(alert, stoppingToken);
            _Logger.LogDebug($"Daily alert {alert.Name} had {parts.Count} players");

            alert.Participants = parts.Count;
            await _AlertDb.UpdateByID(alert.ID, alert);
        }

    }
}
