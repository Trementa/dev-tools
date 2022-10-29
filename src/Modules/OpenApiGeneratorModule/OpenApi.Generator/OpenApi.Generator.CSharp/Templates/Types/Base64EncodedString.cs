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
using System.Linq;

namespace Templates.Types
{
    public partial struct Base64EncodedString
    {
        private byte[] Data { get; }

        private Base64EncodedString(byte[] data) =>
            Data = data;

        public override string ToString() =>
            Convert.ToBase64String(Data);

        public override int GetHashCode() =>
            Data.GetHashCode() + 1;

        public override bool Equals(object obj)
        {
            if (obj is Base64EncodedString base64)
                return Data.SequenceEqual(base64.Data);
            return false;
        }

        public static implicit operator byte[](Base64EncodedString base64) => base64.Data;
        public static explicit operator Base64EncodedString(byte[] data) => new Base64EncodedString(data);

        public static Base64EncodedString Create(string base64) =>
            (Base64EncodedString)Convert.FromBase64String(base64);
    }
}
