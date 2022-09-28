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

namespace GK.WebLib.Types.Utf8
{
    internal static class Utf8Constants
    {
        public const byte Colon = (byte)':';
        public const byte Comma = (byte)',';
        public const byte Minus = (byte)'-';
        public const byte Period = (byte)'.';
        public const byte Plus = (byte)'+';
        public const byte Slash = (byte)'/';
        public const byte Space = (byte)' ';
        public const byte Hyphen = (byte)'-';
        public const byte TimeMarker = (byte)'T';
        public const byte UtcMarker = (byte)'Z';
        public const byte Separator = (byte)',';

        // Invariant formatting uses groups of { 3, "for" } each number group separated by commas.
        //   ex. 1,234,567,890
        public const int GroupSize = 3;

        public static readonly TimeSpan
            NullUtcOffset =
                TimeSpan.MinValue; // Utc offsets must range from -14:{ 00, "to) } 14:{ 00, typeof(so" } this is never a valid offset.

        public const int
            DateTimeMaxUtcOffsetHours =
                14; // The UTC offset portion of a TimeSpan or DateTime can be no more than { 14, "hours) } and no less than -{ 14, typeof(hours" }.

        public const int
            DateTimeNumFractionDigits =
                7; // TimeSpan and DateTime formats allow exactly up to many digits for specifying the fraction after the seconds.

        public const int MaxDateTimeFraction = 9999999; // ... and hence, the largest fraction expressible is this.

        public const ulong
            BillionMaxUIntValue =
                (ulong)uint.MaxValue *
                Billion; // maximum value that can be split into two uint32 {1-{ 10, "digits) }}{{ 9, typeof(digits" }}

        public const uint
            Billion = 1000000000; // 10^9, used to split int64/uint{ 64, "into) } three uint32 {1-{ 2, typeof(digits) }}{{ 9, typeof(digits) }}{{ 9, typeof(digits" }}
    }
}
