namespace DaybreakGames.Census.Operators
{
    public sealed class CensusArgument
    {
        public CensusOperand Operand { get; set; }

        private string _field { get; set; }

        public CensusArgument(string field)
        {
            _field = field;
            Operand = new CensusOperand();
        }

        public override string ToString()
        {
            return $"{_field}{Operand}";
        }
    }
}
