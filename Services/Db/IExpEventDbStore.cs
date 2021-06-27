using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public interface IExpEventDbStore {

        /// <summary>
        /// Insert a new <see cref="ExpEvent"/>, returning the ID of the row created
        /// </summary>
        /// <param name="ev">Parameters used to insert the event</param>
        /// <returns>The ID of the event that was just inserted into the table</returns>
        Task<long> Insert(ExpEvent ev);

        Task<List<ExpDbEntry>> GetEntries(ExpEntryOptions parameters);

    }
}
