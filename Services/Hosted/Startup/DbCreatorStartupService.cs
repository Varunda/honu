using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    /// <summary>
    /// Startup service that runs the DB creator, going thru all patches to ensure the database is ready to run
    /// 
    /// If this fails, the exception is thrown, as honu likely can't run at this point
    /// </summary>
    public class DbCreatorStartupService : IHostedService {

        private readonly ILogger<DbCreatorStartupService> _Logger;
        private readonly IDbCreator _DbCreator;

        public DbCreatorStartupService(ILogger<DbCreatorStartupService> logger,
            IDbCreator dbCreator) {

            _Logger = logger;
            _DbCreator = dbCreator ?? throw new ArgumentNullException(nameof(dbCreator));
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                _Logger.LogInformation($"Starting hosted db creator");
                await _DbCreator.Execute();
                _Logger.LogInformation($"Db creator finished in {timer.ElapsedMilliseconds}ms");

                timer.Stop();
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed DB creation");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
