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

    public class ItemCollection : IItemCollection {

        private readonly ILogger<ItemCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public ItemCollection(ILogger<ItemCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        public Task<PsItem?> GetByID(string itemID) {
            return GetFromCensusByID(itemID, true);
        }

        private async Task<PsItem?> GetFromCensusByID(string itemID, bool retry) {
            CensusQuery query = _Census.Create("item");
            query.Where("item_id").Equals(itemID);

            //_Logger.LogDebug($"Getting item {itemID} from census");

            PsItem? outfit = null;

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    outfit = Parse(result);
                }
            } catch (Exception ex) {
                if (retry == true) {
                    return await GetFromCensusByID(itemID, false);
                } else {
                    _Logger.LogError(ex, "Failed to get item {outfitID}", itemID);
                }
            }

            return outfit;
        }

        private PsItem Parse(JToken result) {
            PsItem item = new PsItem() {
                ID = result.GetString("item_id", "-1"),
                TypeID = result.GetInt32("item_type_id", -1),
                CategoryID = result.GetInt32("item_category_id", -1),
                Name = result.SelectToken("name")?.GetString("en", "<missing name>") ?? "<missing name>"
            };

            return item;
        }

    }
}
