using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models;

namespace watchtower.Services.Repositories {

    /// <summary>
    ///     A repository for getting static game data. This repository will update data from Census, and use the DB to store data
    /// </summary>
    /// <typeparam name="T">Type of game data this repository is for</typeparam>
    public interface IStaticRepository<T> {

        /// <summary>
        ///     Get all data in this repository
        /// </summary>
        /// <returns></returns>
        Task<List<T>> GetAll(CancellationToken cancel = default);

        /// <summary>
        ///     Get a specific <typeparamref name="T"/> with an ID of <paramref name="ID"/>
        /// </summary>
        /// <param name="ID">ID of the entry to get</param>
        /// <returns>
        ///     The <typeparamref name="T"/> with ID of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        Task<T?> GetByID(int ID);

    }

}