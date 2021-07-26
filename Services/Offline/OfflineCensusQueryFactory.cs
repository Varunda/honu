
using System;
using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;

namespace watchtower.Services.Offline {

    public class OfflineCensusQueryFactory : ICensusQueryFactory {

        private readonly ILogger<OfflineCensusQueryFactory> _Logger;

        private readonly ICensusClient _CensusClient;

        public OfflineCensusQueryFactory(ILogger<OfflineCensusQueryFactory> logger,
            ICensusClient client) {

            _Logger = logger;
            _CensusClient = client ?? throw new ArgumentNullException(nameof(client));
        }

        public CensusQuery Create(string serviceName) {
            CensusQuery query = new CensusQuery(_CensusClient, serviceName);

            return query;
        }

    }

}