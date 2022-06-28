using DaybreakGames.Census;
using Microsoft.Extensions.Logging;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class ItemTypeCollection : BaseStaticCollection<ItemType> {

        public ItemTypeCollection(ILogger<BaseStaticCollection<ItemType>> logger,
                ICensusQueryFactory census, ICensusReader<ItemType> reader) 
            : base(logger, "item_type", census, reader) {
        }

    }
}
