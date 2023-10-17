using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.Static;

namespace watchtower.Services.Hosted.Startup {

    /// <summary>
    ///     Runs once at startup, populating all the static directive databases
    /// </summary>
    public class StaticDataPopulator : BackgroundService {

        private readonly ILogger<StaticDataPopulator> _Logger;
        private readonly IServiceProvider _Services;

        public StaticDataPopulator(ILogger<StaticDataPopulator> logger,
            IServiceProvider services) { 

            _Logger = logger;
            _Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(IRefreshableRepository).IsAssignableFrom(p) && p.IsAbstract == false && p.ContainsGenericParameters == false);

                foreach (Type t in types) {
                    _Logger.LogDebug($"refreshing {t.FullName}");
                    IRefreshableRepository repo = (IRefreshableRepository) _Services.GetRequiredService(t);
                    await repo.Refresh(CancellationToken.None);
                }

                _Logger.LogDebug($"finished static data update in {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to update static data");
            }
        }

    }
}
