using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to get data from the /item collection
    /// </summary>
    public class ItemCollection : BaseStaticCollection<PsItem> {

        private readonly ILogger<ItemCollection> _Logger;
        private const string PATCH_FILE = "./census-patches/items.json";

        public ItemCollection(ILogger<ItemCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsItem> reader)
            : base("item", census, reader) {

            _Logger = logger;
        }

        public Task<PsItem?> GetByID(string itemID) {
            CensusQuery query = _Census.Create("item");
            query.Where("item_id").Equals(itemID);

            return _Reader.ReadSingle(query);
        }

        public new async Task<List<PsItem>> GetAll() {
            List<PsItem> items = new List<PsItem>();

            try {
                do {
                    string patch = await File.ReadAllTextAsync(PATCH_FILE);

                    JToken json = JToken.Parse(patch);

                    JToken? itemList = json.SelectToken("item_list");
                    if (itemList == null) {
                        _Logger.LogError($"Missing token 'item_list' from {PATCH_FILE}");
                        break;
                    }

                    IEnumerable<JToken> arr = itemList.Children();

                    foreach (JToken token in arr) {
                        PsItem? item = _Reader.ReadEntry(token);
                        if (item != null) {
                            items.Add(item);
                        } else {
                            _Logger.LogWarning($"got null item from {token}");
                        }
                        //_Logger.LogDebug($"Parsed facility: {fac.Name} => {JToken.FromObject(fac)}");
                    }
                } while (false);
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to patch item");
            }

            return items;
        }

    }
}
