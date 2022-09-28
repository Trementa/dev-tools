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
using System.ComponentModel;

namespace GK.WebLib.Types
{
    public abstract class Primitive<T> : IEquatable<Primitive<T>>
    {
        protected static readonly TypeConverter TypeConverter = TypeDescriptor.GetConverter(typeof(T));
        protected Primitive(T value) => Value = Verify(value);
        protected Primitive(string value) => Value = Verify(Parse(value));

        protected readonly T Value;

        public bool Equals(Primitive<T> p) =>
            p != null && Value.Equals(p.Value);

        protected virtual T Verify(T value) => value;

        public override bool Equals(object obj) =>
            Equals(obj as Primitive<T>);

        public override int GetHashCode() =>
            Value.GetHashCode();

        public override string ToString() =>
            Value.ToString();

        public static bool operator ==(Primitive<T> obj1, Primitive<T> obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null || obj2 is null) return false;
            return obj1.Value.Equals(obj2.Value);
        }

        public static bool operator !=(Primitive<T> obj1, Primitive<T> obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return false;
            if (obj1 is null || obj2 is null) return true;
            return !obj1.Value.Equals(obj2.Value);
        }

        public static explicit operator T(Primitive<T> primitive) => primitive.Value;

        protected internal static T Parse(string value) =>
            (T)TypeConverter.ConvertFromInvariantString(value);
    }
}
