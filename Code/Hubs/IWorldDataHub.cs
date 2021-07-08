using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Code.Hubs {

    public interface IWorldDataHub {

        Task UpdateData(WorldData data);

    }
}
