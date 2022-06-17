using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Alert;

namespace watchtower.Services.Hosted {

    public class HostedDailyAlertCreator : IHostedService {

        private readonly ILogger<HostedDailyAlertCreator> _Logger;
        private readonly IOptions<DailyAlertOptions> _Options;

        public HostedDailyAlertCreator(ILogger<HostedDailyAlertCreator> logger,
            IOptions<DailyAlertOptions> options) {

            _Logger = logger;
            _Options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            _Logger.LogInformation($"{JToken.FromObject( _Options.Value)}");

            foreach (DailyAlertConfigEntry entry in _Options.Value.Worlds) {
                DateTime when = DateTime.UtcNow.Date;
                TimeSpan span = TimeSpan.Parse(entry.When);

                DateTime w = when + span;
                DateTime alertStart = w - TimeSpan.FromDays(1);

                _Logger.LogDebug($"{entry.WorldID} will be generated over from {alertStart:u} to {w:u}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
