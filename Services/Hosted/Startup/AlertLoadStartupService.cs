using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code;
using watchtower.Models.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class AlertLoadStartupService : IHostedService {

        private readonly ILogger<AlertLoadStartupService> _Logger;
        private readonly AlertDbStore _AlertDb;

        public AlertLoadStartupService(ILogger<AlertLoadStartupService> logger,
            AlertDbStore alertDb) {

            _Logger = logger;
            _AlertDb = alertDb;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            List<PsAlert> unfinished = await _AlertDb.LoadUnfinished();

            _Logger.LogInformation($"Loaded {unfinished.Count} unfinished alerts");

            foreach (PsAlert alert in unfinished) {
                AlertStore.Get().AddAlert(alert);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
