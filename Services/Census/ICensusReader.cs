using DaybreakGames.Census.Operators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Census {

    /// <summary>
    ///     Read data from a census query and turn it into a list of objects
    /// </summary>
    /// <typeparam name="T">What the query will be turned into</typeparam>
    public abstract class ICensusReader<T> where T : class {

        /// <summary>
        ///     Read an element from the response returned from Census, and turn it into <typeparamref name="T"/>
        ///     if possible. If the token cannot be turned into <typeparamref name="T"/>, <c>null</c> will be
        ///     returned. It might not be possible to turn into the type if required fields are missing
        /// </summary>
        /// <param name="token">JSON to be turned into a new <typeparamref name="T"/></param>
        /// <returns>
        ///     A new <typeparamref name="T"/> from <paramref name="token"/> if possible,
        ///     or <c>null</c> if <paramref name="token"/> could not be turned into the type
        /// </returns>
        public abstract T? ReadEntry(JToken token);

        /// <summary>
        ///     Take a query and turn it into a list of <typeparamref name="T"/>
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <returns>
        ///     A list of type <typeparamref name="T"/>, generated from the census query passed in <paramref name="query"/>
        /// </returns>
        public async Task<List<T>> ReadList(CensusQuery query) {
            IEnumerable<JToken> tokens = await query.GetListAsync();

            List<T> list = new List<T>();

            foreach (JToken token in tokens) {
                T? elem = ReadEntry(token);
                if (elem != null) {
                    list.Add(elem);
                }
            }

            return list;
        }

        /// <summary>
        ///     Read a single entry from a query
        /// </summary>
        /// <param name="query">Query to be executed</param>
        /// <returns>
        ///     A new type <typeparamref name="T"/>, or null if the read could not be completed 
        /// </returns>
        public async Task<T?> ReadSingle(CensusQuery query) {
            JToken token = await query.GetAsync();

            if (token != null) {
                return ReadEntry(token);
            }

            return null;
        }

    }
}
