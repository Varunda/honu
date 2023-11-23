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

    public class OutfitWarsOutfitCollection {

        private readonly ILogger<OutfitWarsOutfitCollection> _Logger;
        private readonly ICensusReader<OutfitWarsOutfit> _Reader;

        private readonly HttpClient _Http;
        private readonly JsonSerializerOptions _JsonOptions;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY_ALL = "OutfitWarsOutfits.All";

        public OutfitWarsOutfitCollection(ILogger<OutfitWarsOutfitCollection> logger,
            ICensusReader<OutfitWarsOutfit> reader, IMemoryCache cache) {

            _Logger = logger;
            _Reader = reader;

            _Http = new HttpClient();
            _JsonOptions = new JsonSerializerOptions() {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _Cache = cache;
        }

        public async Task<List<OutfitWarsOutfit>> GetAll(CancellationToken cancel) {
            if (_Cache.TryGetValue(CACHE_KEY_ALL, out List<OutfitWarsOutfit> outfits) == false) {
                _Logger.LogInformation($"loading outfit war registration");
                HttpResponseMessage response = await _Http.GetAsync($"https://census.lithafalcon.cc/get/ps2/outfit_war_registration?c:limit=10000&outfit_war_id=]40", cancel);
                DateTime responseEnd = DateTime.UtcNow;

                if (response.StatusCode != HttpStatusCode.OK) {
                    _Logger.LogError($"bad return code: {response.StatusCode}");
                    return new List<OutfitWarsOutfit>();
                }

                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancel);
                JsonElement elem = JsonSerializer.Deserialize<JsonElement>(bytes, _JsonOptions);

                JsonElement? list = elem.GetChild("outfit_war_registration_list");
                if (list == null) {
                    throw new CensusException($"Failed to find 'outfit_war_registration_list' in JsonElement:\n {elem.ToString()[..200]}...");
                }

                outfits = new List<OutfitWarsOutfit>();
                foreach (JsonElement child in list.Value.EnumerateArray()) {
                    OutfitWarsOutfit? o = _Reader.ReadEntry(child);
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
