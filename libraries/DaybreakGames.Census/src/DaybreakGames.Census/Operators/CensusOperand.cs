using System;

namespace DaybreakGames.Census.Operators
{
    public sealed class CensusOperand
    {
        private object _comparator { get; set; }
        private OperatorType _operator { get; set; }

        public void Equals(string value)
        {
            SetEquals(value);
        }

        public void Equals(int value)
        {
            SetEquals(value);
        }

        public void Equals(double value)
        {
            SetEquals(value);
        }

        public void Equals(float value)
        {
            SetEquals(value);
        }

        public void Equals(DateTime value)
        {
            SetEquals(value);
        }

        public void NotEquals(string value)
        {
            SetNotEquals(value);
        }

        public void NotEquals(int value)
        {
            SetNotEquals(value);
        }

        public void NotEquals(double value)
        {
            SetNotEquals(value);
        }

        public void NotEquals(float value)
        {
            SetNotEquals(value);
        }

        public void NotEquals(DateTime value)
        {
            SetNotEquals(value);
        }

        public void IsLessThan(int value)
        {
            SetIsLessThan(value);
        }

        public void IsLessThan(double value)
        {
            SetIsLessThan(value);
        }

        public void IsLessThan(float value)
        {
            SetIsLessThan(value);
        }

        public void IsLessThan(DateTime value)
        {
            SetIsLessThan(value);
        }

        public void IsLessThanOrEquals(int value)
        {
            SetIsLessThanOrEquals(value);
        }

        public void IsLessThanOrEquals(double value)
        {
            SetIsLessThanOrEquals(value);
        }

        public void IsLessThanOrEquals(float value)
        {
            SetIsLessThanOrEquals(value);
        }

        public void IsLessThanOrEquals(DateTime value)
        {
            SetIsLessThanOrEquals(value);
        }

        public void IsGreaterThan(int value)
        {
            SetIsGreaterThan(value);
        }

        public void IsGreaterThan(double value)
        {
            SetIsGreaterThan(value);
        }

        public void IsGreaterThan(float value)
        {
            SetIsGreaterThan(value);
        }

        public void IsGreaterThan(DateTime value)
        {
            SetIsGreaterThan(value);
        }

        public void IsGreaterThanOrEquals(int value)
        {
            SetIsGreaterThanOrEquals(value);
        }

        public void IsGreaterThanOrEquals(double value)
        {
            SetIsGreaterThanOrEquals(value);
        }

        public void IsGreaterThanOrEquals(float value)
        {
            SetIsGreaterThanOrEquals(value);
        }

        public void IsGreaterThanOrEquals(DateTime value)
        {
            SetIsGreaterThanOrEquals(value);
        }

        public void StartsWith(string value)
        {
            SetStartsWith(value);
        }

        public void Contains(string value)
        {
            SetContains(value);
        }

        public override string ToString()
        {
            var mod = "";

            switch (_operator)
            {
                case OperatorType.NotEquals:
                    mod = "!";
                    break;
                case OperatorType.IsLessThan:
                    mod = "<";
                    break;
                case OperatorType.IsLessThanOrEquals:
                    mod = "[";
                    break;
                case OperatorType.IsGreaterThan:
                    mod = ">";
                    break;
                case OperatorType.IsGreaterThanOrEquals:
                    mod = "]";
                    break;
                case OperatorType.StartsWith:
                    mod = "^";
                    break;
                case OperatorType.Contains:
                    mod = "*";
                    break;
            }

            return $"={mod}{GetComparatorString()}";
        }

        private void SetEquals(object value)
        {
            _comparator = value;
            _operator = OperatorType.Equals;
        }

        private void SetNotEquals(object value)
        {
            _comparator = value;
            _operator = OperatorType.NotEquals;
        }

        private void SetIsLessThan(object value)
        {
            _comparator = value;
            _operator = OperatorType.IsLessThan;
        }

        private void SetIsLessThanOrEquals(object value)
        {
            _comparator = value;
            _operator = OperatorType.IsLessThanOrEquals;
        }

        private void SetIsGreaterThan(object value)
        {
            _comparator = value;
            _operator = OperatorType.IsGreaterThan;
        }

        private void SetIsGreaterThanOrEquals(object value)
        {
            _comparator = value;
            _operator = OperatorType.IsGreaterThanOrEquals;
        }

        private void SetStartsWith(object value)
        {
            _comparator = value;
            _operator = OperatorType.StartsWith;
        }

        private void SetContains(object value)
        {
            _comparator = value;
            _operator = OperatorType.Contains;
        }

        private string GetComparatorString()
        {
            if (_comparator is DateTime dtValue)
            {
                return dtValue.ToString("yyyy-MM-dd HH\\:mm\\:ss");
            }

            return _comparator.ToString();
        }

        internal enum OperatorType
        {
            Equals,
            NotEquals,
            IsLessThan,
            IsLessThanOrEquals,
            IsGreaterThan,
            IsGreaterThanOrEquals,
            StartsWith,
            Contains
        }
    }
}
