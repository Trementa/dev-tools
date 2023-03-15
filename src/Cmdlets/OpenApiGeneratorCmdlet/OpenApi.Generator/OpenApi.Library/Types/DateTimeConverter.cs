using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApi.Library.Types;

using static Types.Utf8.DateTimeOFastHandler;

/// <summary>
/// Hard coded to using the 'O' format
/// </summary>
public class DateTimeConverter : JsonConverter<DateTime>
{
    // 'O' == "yyyy-MM-ddTHH:mm:ssZ"
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryParseDateTimeO(reader.ValueSpan, out var value, out _))
            return value;

        var text = reader.GetString();
        if (DateTime.TryParseExact(text, "yyyy-MM-ddTHH:mm:ss.fffZ", null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            return result;
        if (DateTime.TryParse(text, out result))
            return result;

        throw new FormatException($"Could not parse DateTime: \"{text}\"");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        Span<byte> utf8Date = new byte[20];
        value = value.ToUniversalTime();
        if (TryFormatDateTimeO(value, utf8Date, out _))
            writer.WriteStringValue(utf8Date);
        else
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
