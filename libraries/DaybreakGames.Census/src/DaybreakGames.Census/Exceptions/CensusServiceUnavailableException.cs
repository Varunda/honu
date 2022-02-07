namespace DaybreakGames.Census.Exceptions
{
    public class CensusServiceUnavailableException : CensusServerException
    {
        public CensusServiceUnavailableException() : base()
        {
        }

        public CensusServiceUnavailableException(string message) : base(message)
        {
        }
    }
}
