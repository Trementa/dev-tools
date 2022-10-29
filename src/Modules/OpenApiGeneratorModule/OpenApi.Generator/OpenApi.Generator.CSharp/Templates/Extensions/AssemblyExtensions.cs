
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Templates.Extensions;

public static partial class AssemblyExtensions
{
    internal static IEnumerable<Type> ScanAssemblyForType<TAssemblyType, TImp>(params Type[] argumentTypes) =>
        typeof(TAssemblyType)
            .Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(TImp))
                        && !t.IsAbstract
                        && !t.IsInterface
                        && !t.ContainsGenericParameters
                        && t.GetConstructor(
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                            null,
                            CallingConventions.Any,
                            argumentTypes,
                            null) != null);
}