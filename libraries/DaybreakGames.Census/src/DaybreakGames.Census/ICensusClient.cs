using DaybreakGames.Census.Operators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaybreakGames.Census
{
    public interface ICensusClient: IDisposable
    {
        Task<T> ExecuteQuery<T>(CensusQuery query);
        Task<IEnumerable<T>> ExecuteQueryList<T>(CensusQuery query);
        Task<IEnumerable<T>> ExecuteQueryBatch<T>(CensusQuery query);
        Uri CreateRequestUri(CensusQuery query);
    }
}
