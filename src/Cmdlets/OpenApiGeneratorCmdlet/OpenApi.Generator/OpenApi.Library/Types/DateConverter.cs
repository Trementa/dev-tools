using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApi.Library.Types;

using static OpenApi.Library.Types.Utf8.DateOFastHandler;

/// <summary>
/// Hard coded to using the 'O' format
/// </summary>
public class DateConverter : JsonConverter<Date>
{
    // 'O' == "yyyy-MM-dd"
    public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryParseDateO(reader.ValueSpan, out var value, out _))
            return value;

        var text = reader.GetString();
        if (Date.TryParseExact(text, "O", null,
            System.Globalization.DateTimeStyles.RoundtripKind, out var result))
            return result;
        if (Date.TryParse(text, out result))
            return result;

        throw new FormatException($"Could not parse Date: \"{text}\"");
    }

    public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
    {
        Span<byte> utf8Date = new byte[10];

        if (TryFormatDateO(value, utf8Date, out _))
            writer.WriteStringValue(utf8Date);
        else
            writer.WriteStringValue(value.ToString("O"));
    }
}
