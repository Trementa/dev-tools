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

namespace Templates.Types.Utf8
{
    using static OptimizedDecimalWriting;

    internal static class DateTimeOFastHandler
    {
        internal static bool TryParseDateTimeO(ReadOnlySpan<byte> source, out DateTime value, out int bytesConsumed)
        {
            if (source.Length < 20)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int year;
            {
                uint digit1 = source[0] - 48u; // '0'
                uint digit2 = source[1] - 48u; // '0'
                uint digit3 = source[2] - 48u; // '0'
                uint digit4 = source[3] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9 || digit3 > 9 || digit4 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                year = (int)(digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4);
            }

            if (source[4] != Utf8Constants.Hyphen)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int month;
            {
                uint digit1 = source[5] - 48u; // '0'
                uint digit2 = source[6] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                month = (int)(digit1 * 10 + digit2);
            }

            if (source[7] != Utf8Constants.Hyphen)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int day;
            {
                uint digit1 = source[8] - 48u; // '0'
                uint digit2 = source[9] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                day = (int)(digit1 * 10 + digit2);
            }

            if (source[10] != 'T')
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int hour;
            {
                uint digit1 = source[11] - 48u; // '0'
                uint digit2 = source[12] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                hour = (int)(digit1 * 10 + digit2);
            }

            if (source[13] != Utf8Constants.Colon)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int minute;
            {
                uint digit1 = source[14] - 48u; // '0'
                uint digit2 = source[15] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                minute = (int)(digit1 * 10 + digit2);
            }

            if (source[16] != Utf8Constants.Colon)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int second;
            {
                uint digit1 = source[17] - 48u; // '0'
                uint digit2 = source[18] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                second = (int)(digit1 * 10 + digit2);
            }

            if (source[19] != Utf8Constants.UtcMarker)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            bytesConsumed = 20;
            value = new DateTime(year, month, day, hour, minute, second);
            return true;
        }

        internal static bool TryFormatDateTimeO(DateTime value, Span<byte> destination, out int bytesWritten)
        {
            const int MinimumBytesNeeded = 24;
            int bytesRequired = MinimumBytesNeeded;

            if (value.Kind == DateTimeKind.Local || destination.Length < bytesRequired)
            {
                bytesWritten = 0;
                return false;
            }

            bytesWritten = bytesRequired;

            // Hoist most of the bounds checks on buffer.
            {
                var unused = destination[MinimumBytesNeeded - 1];
            }

            WriteFourDecimalDigits((uint)value.Year, destination, 0);
            destination[4] = Utf8Constants.Minus;

            WriteTwoDecimalDigits((uint)value.Month, destination, 5);
            destination[7] = Utf8Constants.Minus;

            WriteTwoDecimalDigits((uint)value.Day, destination, 8);
            destination[10] = Utf8Constants.TimeMarker;

            WriteTwoDecimalDigits((uint)value.Hour, destination, 11);
            destination[13] = Utf8Constants.Colon;

            WriteTwoDecimalDigits((uint)value.Minute, destination, 14);
            destination[16] = Utf8Constants.Colon;

            WriteTwoDecimalDigits((uint)value.Second, destination, 17);
            destination[19] = Utf8Constants.Period;

            WriteThreeDecimalDigits((uint)value.Millisecond, destination, 20);
            destination[23] = Utf8Constants.UtcMarker;

            return true;
        }
    }
}
