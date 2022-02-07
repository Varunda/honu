using DaybreakGames.Census.Operators;

namespace DaybreakGames.Census
{
    public interface ICensusQueryFactory
    {
        CensusQuery Create(string serviceName);
    }
}
