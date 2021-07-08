using DaybreakGames.Census;
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

    public class OutfitCollection : IOutfitCollection {

        private readonly ILogger<OutfitCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public OutfitCollection(ILogger<OutfitCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        public Task<PsOutfit?> GetByID(string outfitID) {
            return GetFromCensusByID(outfitID, true);
        }

        private async Task<PsOutfit?> GetFromCensusByID(string outfitID, bool retry) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("outfit_id").Equals(outfitID);

            //_Logger.LogDebug($"Getting outfit {outfitID} from census");

            query.AddResolve("leader");

            PsOutfit? outfit = null;

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    outfit = Parse(result);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get outfit {outfitID}", outfitID);
                return await GetFromCensusByID(outfitID, false);
            }

            return outfit;
        }

        private PsOutfit Parse(JToken result) {
            PsOutfit outfit = new PsOutfit() {
                ID = result.GetString("outfit_id", "0"),
                Name = result.GetString("name", "<MISSING NAME>"),
                Tag = result.NullableString("alias")
            };

            JToken? leaderToken = result.SelectToken("leader");
            if (leaderToken == null) {
                _Logger.LogWarning($"Missing outfit leader for {outfit.ID}/{outfit.Name}");
            } else {
                outfit.FactionID = leaderToken.GetInt16("faction_id", -1);
            }

            return outfit;
        }

    }
}
