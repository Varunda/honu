using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    /// <summary>
    /// Repository that holds <see cref="WorldData"/> for a world
    /// </summary>
    public interface IWorldDataRepository {

        /// <summary>
        ///     Get the <see cref="WorldData"/> of a world
        /// </summary>
        /// <param name="worldID">ID of the world to get the world data of</param>
        /// <returns>
        ///     The <see cref="WorldData"/> with <see cref="WorldData.WorldID"/> of <paramref name="worldID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        WorldData? Get(short worldID);

        /// <summary>
        ///     Set the <see cref="WorldData"/> of a world
        /// </summary>
        /// <param name="worldID">World ID to set the data of</param>
        /// <param name="data">WorldData to be set</param>
        void Set(short worldID, WorldData data);

    }
}
