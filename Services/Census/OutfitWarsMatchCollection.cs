using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Census {

    public class OutfitWarsMatchCollection {

        private readonly ILogger<OutfitWarsMatchCollection> _Logger;
        private readonly ICensusReader<OutfitWarsMatch> _Reader;

        private readonly HttpClient _Http;
        private readonly JsonSerializerOptions _JsonOptions;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY_ALL = "OutfitWarsMatches.All";

        public OutfitWarsMatchCollection(ILogger<OutfitWarsMatchCollection> logger,
            ICensusReader<OutfitWarsMatch> reader, IMemoryCache cache) {

            _Logger = logger;
            _Reader = reader;

            _Http = new HttpClient();
            _JsonOptions = new JsonSerializerOptions() {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _Cache = cache;
        }

        public async Task<List<OutfitWarsMatch>> GetAll(CancellationToken cancel) {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<OutfitWarsMatch>? outfits) == false || outfits == null) {
                _Logger.LogInformation($"loading outfit war matches");
                HttpResponseMessage response = await _Http.GetAsync($"https://census.lithafalcon.cc/get/ps2/outfit_war_match?c:limit=10000&outfit_war_id=]40", cancel);
                DateTime responseEnd = DateTime.UtcNow;

                if (response.StatusCode != HttpStatusCode.OK) {
                    _Logger.LogError($"bad return code: {response.StatusCode}");
                    return new List<OutfitWarsMatch>();
                }

                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancel);
                JsonElement elem = JsonSerializer.Deserialize<JsonElement>(bytes, _JsonOptions);

                JsonElement? list = elem.GetChild("outfit_war_match_list");
                if (list == null) {
                    throw new CensusException($"Failed to find 'outfit_war_match_list' in JsonElement:\n {elem.ToString()[..200]}...");
                }

                outfits = new List<OutfitWarsMatch>();
                foreach (JsonElement child in list.Value.EnumerateArray()) {
                    OutfitWarsMatch? o = _Reader.ReadEntry(child);
                    if (o != null) {
                        outfits.Add(o);
                    }
                }

                _Cache.Set(CACHE_KEY_ALL, outfits, new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return outfits;
        }

    }
}
