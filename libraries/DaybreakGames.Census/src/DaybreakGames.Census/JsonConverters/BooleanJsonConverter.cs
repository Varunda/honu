using Newtonsoft.Json;
using System;

namespace DaybreakGames.Census.JsonConverters
{
    public class BooleanJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool parseValue;
            if (Boolean.TryParse(reader.Value.ToString(), out parseValue))
            {
                return parseValue;
            }

            return reader.Value.ToString() == "1";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
