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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GK.WebLib.Types.Utf8
{
    internal static class OptimizedDecimalWriting
    {
        /// <summary>
        /// Writes a value [ 0000 .. 9999 ] to the buffer starting at the specified offset.
        /// This method performs best when the starting index is a constant literal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteFourDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
        {
            Debug.Assert(0 <= value && value <= 9999);

            uint temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 3] = (byte)(temp - value * 10);

            temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 2] = (byte)(temp - value * 10);

            temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 1] = (byte)(temp - value * 10);

            buffer[startingIndex] = (byte)('0' + value);
        }

        /// <summary>
        /// Writes a value [ 00 .. 99 ] to the buffer starting at the specified offset.
        /// This method performs best when the starting index is a constant literal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteTwoDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
        {
            Debug.Assert(0 <= value && value <= 99);

            uint temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 1] = (byte)(temp - value * 10);

            buffer[startingIndex] = (byte)('0' + value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteThreeDecimalDigits(uint value, Span<byte> buffer, int startingIndex = 0)
        {
            Debug.Assert(0 <= value && value <= 999);

            uint temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 2] = (byte)(temp - value * 10);

            temp = '0' + value;
            value /= 10;
            buffer[startingIndex + 1] = (byte)(temp - value * 10);

            temp = '0' + value;
            value /= 10;
            buffer[startingIndex] = (byte)(temp - value * 10);
        }
    }
}
