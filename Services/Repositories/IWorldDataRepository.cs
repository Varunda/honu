using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    public interface IWorldDataRepository {

        WorldData? Get(short worldID);

        void Set(short worldID, WorldData data);

    }
}
