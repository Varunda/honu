using Newtonsoft.Json;
using System;

namespace DaybreakGames.Census.JsonConverters
{
    public class DateTimeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Double dValue;
            if (Double.TryParse(reader.Value.ToString(), out dValue))
            {
                return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(dValue);
            }

            DateTime dtValue;
            if (DateTime.TryParse(reader.Value.ToString(), out dtValue))
            {
                return dtValue;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
