using System;
using Newtonsoft.Json;

namespace Shared
{
    // Handles migration of PlayerCooldown fields from legacy double (Unix ms) to DateTime.
    // Old user files store timestamps as double (e.g. 1775448576768.0 or -1.0);
    // new versions serialize as ISO 8601 strings. This converter accepts both on read.
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                double ms = Convert.ToDouble(reader.Value);
                if (ms <= 0) return DateTime.MinValue;
                return DateTimeOffset.FromUnixTimeMilliseconds((long)ms).UtcDateTime;
            }

            if (reader.TokenType == JsonToken.String)
            {
                return DateTime.Parse((string)reader.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }

            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("O"));
        }
    }
}
