using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace watchtower.Code.Converters {
 
    /// <summary>
    /// Custom <c>DateTime</c> converter to avoid changing the format based on <see cref="DateTime.Kind"/>
    /// </summary>
    /// <remarks>
    /// The default <c>DateTime</c> converter uses <see cref="DateTime.Kind"/> to determine the format.
    ///     If it's <see cref="DateTimeKind.Local"/>, it is send as UTC with the timezone offset
    ///     If it's <see cref="DateTimeKind.Unspecified"/>, no 'Z' is added
    ///     If it's <see cref="DateTimeKind.Utc"/>, it's formatted how this is
    ///     
    /// Because all <c>DateTime</c>s are treated as UTC, regardless of the <see cref="DateTime.Kind"/> value,
    ///     this default formatter fails to convert certain <c>DateTime</c>s to the format the frontend expects.
    ///     
    /// This converter assumes all <see cref="DateTime"/>s passed are in UTC, and will always include the 'Z' denoting
    ///     this date string is in UTC (Following the ISO 8601 standard)
    /// </remarks>
    public class DateTimeJsonConverter : JsonConverter<DateTime> {

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return reader.GetDateTime();
        }

        // 
        // turns out the 'u' format is not universal, and Safari does not parse it. the 's' format does not work either, as you need to add
        // the Z yourself which somehow breaks it? Using "s'Z'" you'd get like 23Z, which isn't useful.
        // so this custom format is ideally what we want
        //
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#UniversalSortable
        //
        // Use the 'u' format for the universally sortable format, which is ISO 8601
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
        }

    }

}
