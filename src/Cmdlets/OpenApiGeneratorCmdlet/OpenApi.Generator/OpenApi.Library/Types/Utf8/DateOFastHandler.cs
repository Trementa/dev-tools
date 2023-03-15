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

namespace OpenApi.Library.Types.Utf8
{
    using static OptimizedDecimalWriting;

    internal static class DateOFastHandler
    {
        internal static bool TryParseDateO(ReadOnlySpan<byte> source, out Date value, out int bytesConsumed)
        {
            if (source.Length < 10)
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            int year;
            {
                var digit1 = source[0] - 48u; // '0'
                var digit2 = source[1] - 48u; // '0'
                var digit3 = source[2] - 48u; // '0'
                var digit4 = source[3] - 48u; // '0'

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
                var digit1 = source[5] - 48u; // '0'
                var digit2 = source[6] - 48u; // '0'

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
                var digit1 = source[8] - 48u; // '0'
                var digit2 = source[9] - 48u; // '0'

                if (digit1 > 9 || digit2 > 9)
                {
                    value = default;
                    bytesConsumed = 0;
                    return false;
                }

                day = (int)(digit1 * 10 + digit2);
            }

            value = new Date(year, month, day);
            bytesConsumed = 10;
            return true;
        }

        internal static bool TryFormatDateO(Date value, Span<byte> destination, out int bytesWritten)
        {
            const int MinimumBytesNeeded = 10;

            if (destination.Length < MinimumBytesNeeded)
            {
                bytesWritten = 0;
                return false;
            }

            bytesWritten = MinimumBytesNeeded;

            WriteFourDecimalDigits((uint)value.Year, destination, 0);
            destination[4] = Utf8Constants.Minus;

            WriteTwoDecimalDigits((uint)value.Month, destination, 5);
            destination[7] = Utf8Constants.Minus;

            WriteTwoDecimalDigits((uint)value.Day, destination, 8);
            return true;
        }
    }
}
