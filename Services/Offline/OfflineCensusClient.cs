using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;

namespace watchtower.Services.Offline {

    public class OfflineCensusClient : ICensusClient {

        private readonly ILogger<OfflineCensusClient> _Logger;

        public OfflineCensusClient(ILogger<OfflineCensusClient> logger) {
            _Logger = logger;
        }

        public Uri CreateRequestUri(CensusQuery query) {
            Uri uri = new Uri("");
            return uri;
        }

        public Task<T> ExecuteQuery<T>(CensusQuery query) {
            return Task.FromResult(default(T))!;
        }

        public Task<IEnumerable<T>> ExecuteQueryBatch<T>(CensusQuery query) {
            IEnumerable<T> list = new List<T>();
            return Task.FromResult(list);
        }

        public Task<IEnumerable<T>> ExecuteQueryList<T>(CensusQuery query) {
            IEnumerable<T> list = new List<T>();
            return Task.FromResult(list);
        }

        public void Dispose() { }

    }

}