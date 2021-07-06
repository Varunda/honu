using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public interface IWorldTotalDbStore {

        Task<WorldTotal> Get(WorldTotalOptions options);

    }
}
