using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface IItemDbStore {

        Task<PsItem?> GetByID(string itemID);

        Task Upsert(PsItem item);

    }
}
