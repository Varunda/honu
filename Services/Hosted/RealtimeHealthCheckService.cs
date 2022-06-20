using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class RealtimeHealthCheckService : BackgroundService {

        private readonly ILogger<RealtimeHealthCheckService> _Logger;
        private readonly CensusRealtimeHealthRepository _RealtimeHealthRepository;

        public RealtimeHealthCheckService(ILogger<RealtimeHealthCheckService> logger,
            CensusRealtimeHealthRepository realtimeHealthRepository) {

            _Logger = logger;
            _RealtimeHealthRepository = realtimeHealthRepository;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            throw new System.NotImplementedException();
        }

    }
}
