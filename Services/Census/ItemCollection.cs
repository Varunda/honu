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

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to get data from the /item collection
    /// </summary>
    public class ItemCollection : BaseStaticCollection<PsItem> {

        public ItemCollection(ILogger<ItemCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsItem> reader)
            : base("item", census, reader) { }

        public Task<PsItem?> GetByID(string itemID) {
            CensusQuery query = _Census.Create("item");
            query.Where("item_id").Equals(itemID);

            return _Reader.ReadSingle(query);
        }

    }
}
