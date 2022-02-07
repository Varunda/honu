using System.Collections.Generic;

namespace DaybreakGames.Census.Operators
{
    public sealed class CensusTree : CensusOperator
    {
        private string TreeFieldName { get; set; }
        private List<CensusTree> Tree { get; set; }

        [UriQueryProperty]
        private bool List { get; set; } = false;

        [UriQueryProperty]
        private string Prefix { get; set; }

        [UriQueryProperty]
        private string Start { get; set; }

        public CensusTree(string treeField)
        {
            TreeFieldName = treeField;
            Tree = new List<CensusTree>();
        }

        public void IsList(bool isList)
        {
            List = isList;
        }

        public void GroupPrefix(string prefix)
        {
            Prefix = prefix;
        }

        public void StartField(string field)
        {
            Start = field;
        }

        public CensusTree TreeField(string field)
        {
            if (Tree == null)
            {
                Tree = new List<CensusTree>();
            }

            var newTree = new CensusTree(field);
            Tree.Add(newTree);
            return newTree;
        }

        public override string ToString()
        {
            var baseString = base.ToString();

            if (baseString.Length > 0)
            {
                baseString = $"^{baseString}";
            }

            var subJoinString = "";
            foreach (var subTree in Tree)
            {
                subJoinString += $"({subTree.ToString()})";
            }

            return $"{TreeFieldName}{baseString}{subJoinString}";
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
