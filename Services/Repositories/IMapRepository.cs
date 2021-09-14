using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Repositories {

    public interface IMapRepository {

        Task<List<PsFacility>> GetFacilities();

        Task<List<PsMapHex>> GetHexes();

        Task<List<PsFacilityLink>> GetFacilityLinks();

    }
}
