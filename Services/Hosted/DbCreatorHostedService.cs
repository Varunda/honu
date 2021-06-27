using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    public class DbCreatorHostedService : IHostedService {

        private readonly ILogger<DbCreatorHostedService> _Logger;
        private readonly IDbCreator _DbCreator;

        public DbCreatorHostedService(ILogger<DbCreatorHostedService> logger,
            IDbCreator dbCreator) {

            _Logger = logger;
            _DbCreator = dbCreator ?? throw new ArgumentNullException(nameof(dbCreator));
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            _Logger.LogInformation($"Starting hosted db creator");
            await _DbCreator.Execute();
            _Logger.LogInformation($"Db creator finished");
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
