using DaybreakGames.Census.Operators;

namespace DaybreakGames.Census
{
    public class CensusQueryFactory : ICensusQueryFactory
    {
        private readonly ICensusClient _censusClient;

        public CensusQueryFactory(ICensusClient censusClient)
        {
            _censusClient = censusClient;
        }

        public CensusQuery Create(string serviceName)
        {
            return new CensusQuery(_censusClient, serviceName);
        }
    }
}
