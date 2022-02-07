using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DaybreakGames.Census.Operators
{
    public abstract class CensusOperator
    {
        public abstract string GetKeyValueStringFormat();
        public abstract string GetPropertySpacer();
        public abstract string GetTermSpacer();

        public override string ToString()
        {
            var queryArgs = new List<string>();
            var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(a => a.GetCustomAttribute<UriQueryPropertyAttribute>() != null)
                .ToArray();

            foreach (PropertyInfo prop in properties)
            {
                var uriQueryAttr = prop.GetCustomAttribute<UriQueryPropertyAttribute>();
                var defaultValueAttr = prop.GetCustomAttribute<DefaultValueAttribute>();

                var name = uriQueryAttr.Name;
                var propValue = prop.GetValue(this);

                if (propValue == null)
                {
                    continue;
                }

                var value = GetStringValue(propValue, prop.PropertyType);

                if (defaultValueAttr != null)
                {
                    if (!Equals(defaultValueAttr.Value, propValue))
                    {
                        queryArgs.Add(string.Format(GetKeyValueStringFormat(), name, value));
                    }
                }
                else if (!Equals(propValue, GetDefault(prop.PropertyType)))
                {
                    queryArgs.Add(string.Format(GetKeyValueStringFormat(), name, value));
                }
            }

            if (queryArgs.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(GetPropertySpacer(), queryArgs);
        }

        private static object GetDefault(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private string GetStringValue(object propValue, Type propType)
        {
            if (propType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propType))
            {
                var values = propValue as IEnumerable<object>;
                return string.Join(GetTermSpacer(), values.Select(a => a.ToString()));
            }
            else if (typeof(bool).IsAssignableFrom(propType))
            {
                return propValue.ToString().ToLower();
            }

            return propValue.ToString();
        }
    }
}
