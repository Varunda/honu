using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ItemCategoryCollection : BaseStaticCollection<ItemCategory> {

        public ItemCategoryCollection(ILogger<BaseStaticCollection<ItemCategory>> logger,
                ICensusQueryFactory census, ICensusReader<ItemCategory> reader)
            : base(logger, "item_category", census, reader) {
        }

    }
}
