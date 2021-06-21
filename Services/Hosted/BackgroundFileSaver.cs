using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace watchtower.Services.Hosted {

    /// <summary>
    /// Background service that saves the events periodically. Useful since dotnet watch run does not call
    ///     the shutdown function that usually would do the saving
    /// </summary>
    public class BackgroundFileSaver : BackgroundService {

        private readonly IFileEventLoader _EventLoader;
        private readonly ILogger<BackgroundFileSaver> _Logger;

        public BackgroundFileSaver(IFileEventLoader eventLoader, ILogger<BackgroundFileSaver> logger) {
            _EventLoader = eventLoader ?? throw new ArgumentNullException(nameof(eventLoader));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                while (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogInformation($"Saving in the background");
                    await _EventLoader.Save("PreviousEvents.json");

                    await Task.Delay(30 * 1000, stoppingToken);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, $"Error in BackgroundFilerSaver");

            }
        }

    }
}
