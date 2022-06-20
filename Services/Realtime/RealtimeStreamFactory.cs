using DaybreakGames.Census.Stream;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace watchtower.Services.Realtime {

    public class RealtimeStreamFactory {

        private readonly ILogger<RealtimeStreamFactory> _Logger;

        private readonly IServiceProvider _Services;

        public RealtimeStreamFactory(ILogger<RealtimeStreamFactory> logger,
            IServiceProvider services) {

            _Logger = logger;
            _Services = services;
        }

        public ICensusStreamClient New() {
            ICensusStreamClient? stream = _Services.GetService<ICensusStreamClient>();
            if (stream == null) {
                throw new SystemException($"No service for stream");
            }

            return stream;
        }

    }
}
