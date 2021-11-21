using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterCollection : ICharacterCollection {

        private readonly ILogger<CharacterCollection> _Logger;

        private readonly ICensusQueryFactory _Census;

        public CharacterCollection(ILogger<CharacterCollection> logger,
                ICensusQueryFactory factory) {

            _Logger = logger;
            _Census = factory;
        }

        public async Task<PsCharacter?> GetByName(string name) {
            PsCharacter? c = await _GetCharacterFromCensusByName(name, true);

            return c;
        }

        public async Task<PsCharacter?> GetByID(string ID) {
            PsCharacter? c = await _GetCharacterFromCensus(ID, true);

            return c;
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
