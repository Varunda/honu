using System.Collections.Generic;
using System.ComponentModel;

namespace DaybreakGames.Census.Operators
{
    public sealed class CensusJoin : CensusOperator
    {
        [UriQueryProperty]
        private bool List { get; set; } = false;

        [DefaultValue(true)]
        [UriQueryProperty]
        private bool Outer { get; set; } = true;

        [UriQueryProperty]
        private IEnumerable<string> Show { get; set; }

        [UriQueryProperty]
        private IEnumerable<string> Hide { get; set; }

        [UriQueryProperty]
        private IEnumerable<CensusArgument> Terms { get; set; }

        [UriQueryProperty]
        private string On { get; set; }

        [UriQueryProperty]
        private string To { get; set; }

        [UriQueryProperty("inject_at")]
        private string InjectAt { get; set; }

        private List<CensusJoin> Join { get; set; } = new List<CensusJoin>();

        private string Service;

        public CensusJoin(string service)
        {
            Service = service;
        }

        public void IsList(bool isList)
        {
            List = isList;
        }

        public void IsOuterJoin(bool isOuter)
        {
            Outer = isOuter;
        }

        public void ShowFields(params string[] fields)
        {
            Show = fields;
        }

        public void HideFields(params string[] fields)
        {
            Hide = fields;
        }

        public void OnField(string field)
        {
            On = field;
        }

        public void ToField(string field)
        {
            To = field;
        }

        public void WithInjectAt(string field)
        {
            InjectAt = field;
        }

        public CensusOperand Where(string field)
        {
            var arg = new CensusArgument(field);

            if (Terms == null)
            {
                Terms = new List<CensusArgument>();
            }

            var terms = Terms as List<CensusArgument>;
            terms.Add(arg);
            Terms = terms;

            return arg.Operand;
        }

        public CensusJoin JoinService(string service)
        {
            var newJoin = new CensusJoin(service);
            Join.Add(newJoin);
            return newJoin;
        }

        public override string ToString()
        {
            var baseString = base.ToString();

            if (baseString.Length > 0)
            {
                baseString = $"^{baseString}";
            }

            var subJoinString = "";
            foreach (var subJoin in Join)
            {
                subJoinString += $"({subJoin.ToString()})";
            }

            return $"{Service}{baseString}{subJoinString}";
        }

        public override string GetKeyValueStringFormat()
        {
            return "{0}:{1}";
        }

        public override string GetPropertySpacer()
        {
            return "^";
        }

        public override string GetTermSpacer()
        {
            return "'";
        }
    }
}
