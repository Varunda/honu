using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using honu_census;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterCollection : ICharacterCollection {

        private readonly ILogger<CharacterCollection> _Logger;

        private readonly ICensusQueryFactory _Census;
        private readonly HonuCensus _HCensus;

        private const int BATCH_SIZE = 50;

        public CharacterCollection(ILogger<CharacterCollection> logger,
                ICensusQueryFactory factory, HonuCensus hc) {

            _Logger = logger;
            _Census = factory;
            _HCensus = hc ?? throw new ArgumentNullException(nameof(hc));
        }

        public async Task<PsCharacter?> GetByName(string name) {
            _HCensus.AddServiceId("asdf");
            honu_census.Models.CensusQuery q = _HCensus.New("character");
            q.WhereEquals("name.first_lower", name.ToLower());

            JToken? ttt = await _HCensus.GetSingle(q, CancellationToken.None);
            if (ttt != null) {
                PsCharacter? cc = _ParseCharacter(ttt);
                return cc;
            }

            PsCharacter? c = await _GetCharacterFromCensusByName(name, true);

            return c;
        }

        public async Task<PsCharacter?> GetByID(string ID) {
            PsCharacter? c = await _GetCharacterFromCensus(ID, true);

            return c;
        }

        public async Task<List<PsCharacter>> GetByIDs(List<string> IDs) {
            int batchCount = (int) Math.Ceiling(IDs.Count / (double) BATCH_SIZE);

            //_Logger.LogTrace($"Doing {batchCount} batches to get {IDs.Count} characters");

            List<PsCharacter> chars = new List<PsCharacter>(IDs.Count);

            for (int i = 0; i < batchCount; ++i) {
                //_Logger.LogTrace($"Getting indexes {i * BATCH_SIZE} - {i * BATCH_SIZE + BATCH_SIZE}");
                List<string> slice = IDs.Skip(i * BATCH_SIZE).Take(BATCH_SIZE).ToList();

                //_Logger.LogTrace($"Slize size: {slice.Count}");

                CensusQuery query = _Census.Create("character");
                foreach (string id in slice) {
                    query.Where("character_id").Equals(id);
                }
                query.SetLimit(10_000);
                query.AddResolve("outfit", "world");

                // If there is an exception, ignore census connection ones
                try {
                    IEnumerable<JToken> result = await query.GetListAsync();

                    foreach (JToken token in result) {
                        PsCharacter? c = _ParseCharacter(token);
                        if (c != null) {
                            chars.Add(c);
                        }
                    }
                } catch (CensusConnectionException) {
                    _Logger.LogWarning($"Failed to get slice {i * BATCH_SIZE} - {(i + 1) * BATCH_SIZE}");
                    continue;
                }
            }

            return chars;
        }

        public async Task<List<PsCharacter>> SearchByName(string name, CancellationToken stop) {
            // Cannot search less than 3 characters in Census
            if (name.Length < 3) {
                return new List<PsCharacter>();
            }

            CensusQuery query = _Census.Create("character");
            query.Where("name.first_lower").Contains(name);
            query.SetLimit(100);
            query.AddResolve("outfit", "world");

            List<PsCharacter> chars;

            try {
                IEnumerable<JToken> result = await query.GetListAsync();
                chars = new List<PsCharacter>(result.Count());

                foreach (JToken token in result) {
                    PsCharacter? c = _ParseCharacter(token);
                    if (c != null) {
                        chars.Add(c);
                    }
                }
            } catch (Exception) when (stop.IsCancellationRequested == false) {
                throw;
            } catch (Exception) when (stop.IsCancellationRequested == true) {
                _Logger.LogWarning($"Stopping name search");
                return new List<PsCharacter>();
            }

            return chars;
        }

        private async Task<PsCharacter?> _GetCharacterFromCensus(string ID, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("character_id").Equals(ID);
            query.AddResolve("outfit", "world");

            try {
                JToken result = await query.GetAsync();

                PsCharacter? player = _ParseCharacter(result);

                return player;
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", ID);
                    return await _GetCharacterFromCensus(ID, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", ID);
                    throw;
                }
            }
        }

        private async Task<PsCharacter?> _GetCharacterFromCensusByName(string name, bool retry) {
            CensusQuery query = _Census.Create("character");

            query.Where("name.first_lower").Equals(name.ToLower());
            query.AddResolve("outfit", "world");

            try {
                JToken result = await query.GetAsync();

                PsCharacter? player = _ParseCharacter(result);

                return player;
            } catch (CensusConnectionException ex) {
                if (retry == true) {
                    _Logger.LogWarning("Retrying {Char} from API", name);
                    return await _GetCharacterFromCensusByName(name, false); 
                } else {
                    _Logger.LogError(ex, "Failed to get {0} from API", name);
                    throw;
                }
            }
        }

        private PsCharacter? _ParseCharacter(JToken result) {
            if (result == null) {
                return null;
            }

            PsCharacter player = new PsCharacter {
                ID = result.GetString("character_id", "0"),
                FactionID = result.GetInt16("faction_id", -1),
                Prestige = result.GetInt32("prestige_level", 0),
                WorldID = result.GetWorldID()
            };

            JToken? nameToken = result.SelectToken("name");
            if (nameToken == null) {
                _Logger.LogWarning($"Missing name field from {result}");
            } else {
                player.Name = nameToken.Value<string?>("first") ?? "BAD NAME";
            }

            JToken? times = result.SelectToken("times");
            if (times != null) {
                player.DateCreated = times.CensusTimestamp("creation");
                player.DateLastLogin = times.CensusTimestamp("last_login");
                player.DateLastSave = times.CensusTimestamp("last_save");
            }

            player.OutfitID = result.SelectToken("outfit")?.Value<string?>("outfit_id");
            player.OutfitName = result.SelectToken("outfit")?.Value<string?>("name");
            player.OutfitTag = result.SelectToken("outfit")?.Value<string?>("alias");
            player.BattleRank = result.SelectToken("battle_rank")?.GetInt16("value", 0) ?? 0;

            return player;
        }

    }
}
