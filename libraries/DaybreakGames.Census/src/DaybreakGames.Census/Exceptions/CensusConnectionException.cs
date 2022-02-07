using System;

namespace DaybreakGames.Census.Exceptions
{
    public class CensusConnectionException : CensusException
    {
        public CensusConnectionException() : base()
        {
        }

        public CensusConnectionException(string message) : base(message)
        {
        }
    }
}
