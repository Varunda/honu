using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Census;

namespace watchtower.Services.Hosted {

    public class HostedFileEventLoader : BackgroundService {

        private const string _EventsFile = "PreviousEvents.json";

        private readonly ILogger<HostedFileEventLoader> _Logger;
        private readonly IFileEventLoader _Loader;

        public HostedFileEventLoader(ILogger<HostedFileEventLoader> logger,
            IFileEventLoader loader) {

            _Logger = logger;

            _Loader = loader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                await _Loader.Load(_EventsFile);
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to load events");
            }
        }

    }
}
