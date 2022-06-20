using DaybreakGames.Census.Stream;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Websocket.Client;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Wrapper around the information of a realtime stream. Sealed so Dispose() doesn't need that weird GC thing
    /// </summary>
    public sealed class RealtimeStream : IDisposable {

        public readonly string Name;

        public readonly ICensusStreamClient Client;

        public List<CensusStreamSubscription> Subscriptions { get; set; } = new();

        public DateTime LastConnect { get; set; }

        public RealtimeStream(string name, ICensusStreamClient client) {
            Name = name;
            Client = client;
        }

        public void Dispose() {
            Client.Dispose();
        }

    }
}
