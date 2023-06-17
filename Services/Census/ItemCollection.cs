using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to get data from the /item collection
    /// </summary>
    public class ItemCollection : BaseStaticCollection<PsItem> {

        //private readonly ILogger<ItemCollection> _Logger;
        private const string PATCH_FILE = "./census-patches/items.json";

        public ItemCollection(ILogger<ItemCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsItem> reader)
            : base(logger, "item", census, reader) {

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
                    JsonElement readElement(byte[] file) {
                        Utf8JsonReader reader = new(file);
                        JsonElement patchedJson = JsonElement.ParseValue(ref reader);
                        return patchedJson;
                    }

                    byte[] bytes = await File.ReadAllBytesAsync(PATCH_FILE);
                    JsonElement json = readElement(bytes);
                    JsonElement? itemList = json.GetChild("item_list");
                    if (itemList == null) {
                        _Logger.LogError($"Missing token 'item_list' from {PATCH_FILE}");
                        break;
                    }

                    IEnumerable<JsonElement> arr = itemList.Value.EnumerateArray();
                    foreach (JsonElement token in arr) {
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
