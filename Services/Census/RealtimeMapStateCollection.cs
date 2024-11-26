using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Db;

namespace watchtower.Services.Census {

    public class RealtimeMapStateCollection {

        private readonly ILogger<RealtimeMapStateCollection> _Logger;
        private readonly ICensusReader<RealtimeMapState> _Reader;

        private readonly HttpClient _Http;
        private readonly JsonSerializerOptions _JsonOptions;

        public RealtimeMapStateCollection(ILogger<RealtimeMapStateCollection> logger,
            ICensusReader<RealtimeMapState> reader) {

            _Logger = logger;
            _Reader = reader;

            _Http = new HttpClient();
            _JsonOptions = new JsonSerializerOptions() {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<RealtimeMapState>> GetAll(CancellationToken cancel) {
            string worldIDs = string.Join(",", World.PcStreams);
            string zoneIDs = string.Join(",", Zone.StaticZones);

            HttpResponseMessage response = await _Http.GetAsync($"https://census.lithafalcon.cc/get/ps2/map_state?world_id={worldIDs}&zone_id={zoneIDs}&c:limit=10000", cancel);
            DateTime responseEnd = DateTime.UtcNow;

            if (response.StatusCode != HttpStatusCode.OK) {
                return new List<RealtimeMapState>();
            }

            byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancel);
            JsonElement elem = JsonSerializer.Deserialize<JsonElement>(bytes, _JsonOptions);

            JsonElement? list = elem.GetChild("map_state_list");
            if (list == null) {
                throw new CensusException($"Failed to find 'map_state_list' in JsonElement:\n {elem.ToString()[..200]}...");
            }

            List<RealtimeMapState> mapState = new();
            foreach (JsonElement child in list.Value.EnumerateArray()) {
                RealtimeMapState? state = _Reader.ReadEntry(child);
                if (state != null) {
                    state.SaveTimestamp = responseEnd;
                    mapState.Add(state);
                }
            }

            return mapState;
        }

    }
}
