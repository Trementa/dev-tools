using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace OpenApi.Generator;

public static partial class Extensions
{
    public static IConfigurationBuilder AddCommandLineMapping(this IConfigurationBuilder me, string[] args) =>
        me.AddCommandLine(args, Options.SwitchMappings);

    public static void LogError<T>(this Logger logger, IEnumerable<T> listOfErrors, Func<T, object> mapProperty)
    {
        foreach (var error in listOfErrors)
            logger.LogError(mapProperty(error).ToString());
    }

    public static void LogError<T>(this Logger logger, IEnumerable<T> listOfErrors)
    {
        foreach (var error in listOfErrors)
            logger.LogError(error.ToString());
    }

    public static string ToPascalCase(this string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return string.Empty;

        identifier = string.Join(' ', Regex.Split(identifier, @"(?<!^)(?=[A-Z])"));
        var resultBuilder = new StringBuilder();
        var appendedSpace = false;

        foreach (char c in identifier)
        {
            if (!char.IsLetterOrDigit(c))
            {
                resultBuilder.Append(" ");
                appendedSpace = true;
            }
            else
                resultBuilder.Append(c);
        }

        if (!appendedSpace)
        {
            var res = resultBuilder.ToString();
            return $"{char.ToUpperInvariant(res[0])}{res.Substring(1)}";
        }

        var result = resultBuilder.ToString().ToLower();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(result).Replace(" ", String.Empty);
    }

    public static string ToCamelCase(this string identifier)
    {
        var result = identifier.ToPascalCase();
        if (string.IsNullOrWhiteSpace(result))
            return result;
        return $"{char.ToLowerInvariant(result[0])}{identifier.Substring(1)}";
    }
}
