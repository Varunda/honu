using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface IMapDbStore {

        Task<List<PsMapHex>> GetHexes();

        Task<List<PsFacilityLink>> GetFacilityLinks();

        Task UpsertHex(PsMapHex hex);

        Task UpsertLink(PsFacilityLink link);

    }

}
