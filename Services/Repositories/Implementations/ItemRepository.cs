using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories.Implementations {

    public class ItemRepository : IItemRepository {

        private readonly ILogger<ItemRepository> _Logger;
        private readonly IMemoryCache _Cache;

        private const string _CacheKeyID = "Item.ID.{0}"; // {0} => Item ID

        private readonly IItemDbStore _Db;
        private readonly IItemCollection _Census;

        public ItemRepository(ILogger<ItemRepository> logger, IMemoryCache cache,
            IItemDbStore db, IItemCollection coll) {

            _Logger = logger;
            _Cache = cache;

            _Db = db ?? throw new ArgumentNullException(nameof(db));
            _Census = coll ?? throw new ArgumentNullException(nameof(coll));
        }

        public async Task<PsItem?> GetByID(string itemID) {
            if (itemID == "") {
                return null;
            }

            if (itemID == "0") {
                return PsItem.NoItem;
            }

            string key = string.Format(_CacheKeyID, itemID);

            if (_Cache.TryGetValue(key, out PsItem? item) == false) {
                item = await _Db.GetByID(itemID);

                if (item == null) {
                    PsItem? censusItem = await _Census.GetByID(itemID);
                    if (censusItem != null) {
                        item = censusItem;
                    }

                    if (item != null) {
                        await _Db.Upsert(item);
                    }
                }

                _Cache.Set(key, item, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                });
            }

            return item;
        }

        public Task Upsert(PsItem outfit) {
            throw new NotImplementedException();
        }

    }
}
