using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface IItemRepository {

        Task<PsItem?> GetByID(string itemID);

        Task Upsert(PsItem outfit);

    }
}
