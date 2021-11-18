using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db {

    public interface IBattleRankDbStore {

        Task Insert(string charID, int rank, DateTime timestamp);

    }
}
