using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public interface IFacilityDbStore {

        /// <summary>
        ///     Get a <see cref="PsFacility"/> from the backing db by the <see cref="PsFacility.FacilityID"/>
        /// </summary>
        /// <param name="facilityID">Facility ID of the <see cref="PsFacility"/> to get</param>
        /// <returns>
        ///     The <see cref="PsFacility"/> with <see cref="PsFacility.FacilityID"/> of <paramref name="facilityID"/>,
        ///     or <c>null</c> if it does not exist in the database, but it may exist in Census
        /// </returns>
        Task<PsFacility?> GetByFacilityID(int facilityID);

        /// <summary>
        ///     Get all <see cref="PsFacility"/>s in the DB
        /// </summary>
        /// <returns>
        ///     A list of all <see cref="PsFacility"/> currently in the DB
        /// </returns>
        Task<List<PsFacility>> GetAll();

        /// <summary>
        ///     Insert or Update (upsert) a facility
        /// </summary>
        /// <param name="facilityID">ID of the facility to be upserted</param>
        /// <param name="facility">Values used when performing the insert/update</param>
        Task Upsert(int facilityID, PsFacility facility);

    }
}
