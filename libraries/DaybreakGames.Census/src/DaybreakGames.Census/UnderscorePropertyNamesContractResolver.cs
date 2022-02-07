using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DaybreakGames.Census
{
    public class UnderscorePropertyNamesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (!member.CustomAttributes.Any(a => a.AttributeType == typeof(JsonPropertyAttribute)))
            {
                property.PropertyName = Regex.Replace(property.PropertyName, @"(\w)([A-Z])", "$1_$2").ToLower();
            }

            return property;
        }
    }
}
