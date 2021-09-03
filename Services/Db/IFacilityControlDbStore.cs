using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public interface IFacilityControlDbStore {

        /// <summary>
        /// Insert a new <see cref="FacilityControlEvent"/> into the DB
        /// </summary>
        /// <param name="ev">Event to store</param>
        Task Insert(FacilityControlEvent ev);

        Task<List<FacilityControlDbEntry>> Get(FacilityControlOptions parameters);

    }
}
