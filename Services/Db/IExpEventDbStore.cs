using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public interface IExpEventDbStore {

        /// <summary>
        ///     Insert a new <see cref="ExpEvent"/>, returning the ID of the row created
        /// </summary>
        /// <param name="ev">Parameters used to insert the event</param>
        /// <returns>
        ///     The ID of the event that was just inserted into the table
        /// </returns>
        Task<long> Insert(ExpEvent ev);

        /// <summary>
        ///     Get the top players who have performed an action specified in <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters">Parameters used to performed the action</param>
        /// <returns>
        ///     The top players who have met the parameters passed in <paramref name="parameters"/>
        /// </returns>
        Task<List<ExpDbEntry>> GetEntries(ExpEntryOptions parameters);

        /// <summary>
        ///     Get the outfits who have performed an action specified in <paramref name="options"/>
        /// </summary>
        /// <param name="options">Options to filter the entries returned</param>
        /// <returns>
        ///     A list of outfits who have met the parameters passed in <paramref name="options"/>
        /// </returns>
        Task<List<ExpDbEntry>> GetTopOutfits(ExpEntryOptions options);

    }
}
