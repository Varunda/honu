using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted.Startup {

    public class AlertLoadStartupService : IHostedService {

        private readonly ILogger<AlertLoadStartupService> _Logger;
        private readonly AlertDbStore _AlertDb;
        private readonly MetagameEventRepository _MetagameRepository;

        public AlertLoadStartupService(ILogger<AlertLoadStartupService> logger,
            AlertDbStore alertDb, MetagameEventRepository metagameRepository) {

            _Logger = logger;
            _AlertDb = alertDb;
            _MetagameRepository = metagameRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            List<PsAlert> unfinished = await _AlertDb.LoadUnfinished();
            Dictionary<int, PsMetagameEvent> events = (await _MetagameRepository.GetAll()).ToDictionary(iter => iter.ID);

            _Logger.LogInformation($"Loaded {unfinished.Count} unfinished alerts");

            foreach (PsAlert alert in unfinished) {
                AlertStore.Get().AddAlert(alert);

                lock (ZoneStateStore.Get().Zones) {
                    ZoneState? state = ZoneStateStore.Get().GetZone(alert.WorldID, alert.ZoneID);
                    if (state == null) {
                        state = new ZoneState() {
                            ZoneID = alert.ZoneID,
                            WorldID = alert.WorldID,
                            IsOpened = true
                        };
                    }

                    state.Alert = alert;
                    _ = events.TryGetValue(alert.AlertID, out PsMetagameEvent? ev);
                    state.AlertInfo = ev;
                    state.AlertStart = alert.Timestamp;
                    state.AlertEnd = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);

                    ZoneStateStore.Get().SetZone(alert.WorldID, alert.ZoneID, state);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
