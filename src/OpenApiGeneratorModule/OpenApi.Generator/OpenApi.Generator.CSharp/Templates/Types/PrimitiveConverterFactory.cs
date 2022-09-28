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
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GK.WebLib.Types
{
    public class PrimitiveConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            IsSubclassOfRawGeneric(typeof(Primitive<>), typeToConvert)
            && !typeToConvert.IsAbstract
            && !typeToConvert.IsInterface;

        protected bool IsSubclassOfRawGeneric(Type generic, Type toCheck) =>
            GetGenericArgumentsForSubclassOfGeneric(generic, toCheck).Length > 0;

        protected internal Type[] GetGenericArgumentsForSubclassOfGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return toCheck.GetGenericArguments();
                }
                toCheck = toCheck.BaseType;
            }
            return new Type[0];
        }

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) =>
            PrimitiveConverterCache.GetOrAdd(type, typeToConvert =>
            {
                var primitiveType = GetGenericArgumentsForSubclassOfGeneric(typeof(Primitive<>), typeToConvert)[0];
                return (JsonConverter)Activator
                    .CreateInstance(typeof(PrimitiveConverter<,>)
                        .MakeGenericType(typeToConvert, primitiveType), options);
            });

        protected readonly ConcurrentDictionary<Type, JsonConverter> PrimitiveConverterCache = new ConcurrentDictionary<Type, JsonConverter>();

        protected internal class PrimitiveConverter<T1, T2> : JsonConverter<T1> where T1 : Primitive<T2>
        {
            protected readonly JsonSerializerOptions JsonSerializerOptions;
            public PrimitiveConverter(JsonSerializerOptions options) =>
                JsonSerializerOptions = options;

            public override T1 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var typeConverter = TypeDescriptor.GetConverter(typeof(T2));
                var primitiveValue = (T2)typeConverter.ConvertFromString(reader.GetString());
                return (T1)Activator.CreateInstance(typeToConvert,
                    BindingFlags.CreateInstance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.OptionalParamBinding,
                    null, new object[] { primitiveValue }, CultureInfo.CurrentCulture);
            }

            public override void Write(Utf8JsonWriter writer, T1 value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
