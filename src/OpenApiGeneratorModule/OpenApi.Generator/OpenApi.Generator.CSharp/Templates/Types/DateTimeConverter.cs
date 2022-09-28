/*
MIT License

Copyright (c) 2020 Trementa AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GK.WebLib.Types
{
    using static GK.WebLib.Types.Utf8.DateTimeOFastHandler;

    /// <summary>
    /// Hard coded to using the 'O' format
    /// </summary>
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        // 'O' == "yyyy-MM-ddTHH:mm:ssZ"
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (TryParseDateTimeO(reader.ValueSpan, out DateTime value, out _))
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
}
