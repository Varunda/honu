namespace DaybreakGames.Census
{
    public class CensusOptions
    {
        public string CensusServiceId { get; set; } = Constants.DefaultServiceId;
        public string CensusServiceNamespace { get; set; } = Constants.DefaultServiceNamespace;
        public bool LogCensusErrors { get; set; } = false;
    }
}
