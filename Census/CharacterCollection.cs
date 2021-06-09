using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Census {

    public class CharacterCollection : ICharacterCollection {

        private readonly ILogger<CharacterCollection> _Logger;

        private readonly ICensusQueryFactory _Census;

        private readonly ConcurrentDictionary<string, Character?> _Cache = new ConcurrentDictionary<string, Character?>();

        private List<string> _Pending = new List<string>();

        public CharacterCollection(ILogger<CharacterCollection> logger,
            IMemoryCache cache, ICensusQueryFactory factory) {

            _Logger = logger;

            _Census = factory;
        }

        public void Cache(string ID) {
            if (_Cache.TryGetValue(ID, out Character _) == false) {
                if (_Pending.FindIndex(iter => iter == ID) > -1) {
                    _Logger.LogTrace($"Skipping caching of pending character {ID}");
                    return;
                }
                _ = GetByIDAsync(ID);
            }
        }

        public async Task<Character?> GetByNameAsync(string name) {
            Character? c = GetCache().FirstOrDefault(iter => iter.Name.ToLower() == name.ToLower());
            if (c != null) {
                return c;
            }

            c = await _GetCharacterFromCensusByName(name, true);
            if (c != null) {
                _Cache.TryAdd(c.ID, c);
            }
            return c;
        }

        public List<Character> GetCache() {
            // Force is safe
            return _Cache.Values.Where(iter => iter != null).ToList()!;
        }

        public async Task<Character?> GetByIDAsync(string ID) {
            if (_Cache.TryGetValue(ID, out Character? player) == false) {
                _Pending.Add(ID);

                player = await _GetCharacterFromCensus(ID, true);
                if (player == null) {
                    _Logger.LogWarning($"Failed to get character {ID}");
                }

                _Cache.TryAdd(ID, player);

                _Pending.Remove(ID);
            }

            return player;
        }

        public async Task CacheBlock(List<string> IDs) {
            _Logger.LogInformation($"Caching {IDs.Count} characters");

            int blockSize = 20;

            for (int i = 0; i < IDs.Count; i += blockSize) {
                int count = Math.Min(blockSize, IDs.Count - i);
                List<string> block = IDs.GetRange(i, count);

                CensusQuery query = _Census.Create("character");
                foreach (string ID in block) {
                    query.Where("character_id").Equals(ID);
                }
                query.AddResolve("outfit");
                query.AddResolve("outfit", "online_status");
                query.ShowFields("character_id", "name", "faction_id", "outfit", "online_status");

                List<JToken> tokens = (await query.GetListAsync()).ToList();
                foreach (JToken token in tokens) {
                    Character? c = _ParseCharacter(token);
                    if (c != null) {
                        _Cache.TryAdd(c.ID, c);
                    }
                }

                //_Logger.LogInformation($"Cached {i}/{IDs.Count} characters");
            }
            _Logger.LogInformation($"Cached all characters");
        }

        private async Task<Character?> _GetCharacterFromCensus(string ID, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("character_id").Equals(ID);
            query.AddResolve("outfit", "online_status");
            query.ShowFields("character_id", "name", "faction_id", "outfit", "online_status");

            try {
                JToken result = await query.GetAsync();

                Character? player = _ParseCharacter(result);

                return player;
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", ID);
                    return await _GetCharacterFromCensus(ID, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", ID);
                    throw ex;
                }
            }
        }

        private async Task<Character?> _GetCharacterFromCensusByName(string name, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("name.first_lower").Equals(name.ToLower());
            query.AddResolve("outfit");

            try {
                JToken result = await query.GetAsync();

                Character? player = _ParseCharacter(result);

                return player;
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", name);
                    return await _GetCharacterFromCensusByName(name, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", name);
                    throw ex;
                }
            }
        }

        private Character? _ParseCharacter(JToken result) {
            if (result == null) {
                return null;
            }

            Character player = new Character {
                ID = result.Value<string?>("character_id") ?? "0",
                FactionID = result.Value<string?>("faction_id") ?? "-1",
                Online = (result.Value<string?>("online_status") ?? "0") != "0"
            };

            JToken? nameToken = result.SelectToken("name");
            if (nameToken == null) {
                _Logger.LogWarning($"Missing name field from {result}");
            } else {
                player.Name = nameToken.Value<string?>("first") ?? "BAD NAME";
            }

            JToken? outfitToken = result.SelectToken("outfit");
            if (outfitToken != null) {
                player.OutfitID = outfitToken.Value<string?>("outfit_id");
                player.OutfitTag = outfitToken.Value<string?>("alias");
                player.OutfitName = outfitToken.Value<string?>("name");
            }

            return player;
        }

    }
}
