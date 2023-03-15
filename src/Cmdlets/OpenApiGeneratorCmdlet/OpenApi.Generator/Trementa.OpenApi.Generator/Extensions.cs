using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Trementa.OpenApi.Generator;

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

        foreach (var c in identifier)
            if (!char.IsLetterOrDigit(c))
            {
                resultBuilder.Append(" ");
                appendedSpace = true;
            }
            else
                resultBuilder.Append(c);

        if (!appendedSpace)
        {
            var res = resultBuilder.ToString();
            return $"{char.ToUpperInvariant(res[0])}{res.Substring(1)}";
        }

        var result = resultBuilder.ToString().ToLower();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(result).Replace(" ", string.Empty);
    }

    public static string ToCamelCase(this string identifier)
    {
        var result = identifier.ToPascalCase();
        if (string.IsNullOrWhiteSpace(result))
            return result;
        return $"{char.ToLowerInvariant(result[0])}{identifier.Substring(1)}";
    }

    public static OutputTypeEnum GetOutputType(this IConfiguration me)
    {
        var outputType = me["OutputType"];

        if (string.IsNullOrWhiteSpace(outputType))
            return OutputTypeEnum.None;

        var outputTypeEnum = (OutputTypeEnum)0;
        var splits = outputType.Split('|');
        foreach (var split in splits)
        {
            var val = split.Trim();
            if (string.Equals(val, nameof(OutputTypeEnum.SDK), StringComparison.CurrentCultureIgnoreCase))
                outputTypeEnum |= OutputTypeEnum.SDK;
            else if (string.Equals(val, nameof(OutputTypeEnum.Model), StringComparison.CurrentCultureIgnoreCase))
                outputTypeEnum |= OutputTypeEnum.Model;
            else if (string.Equals(val, nameof(OutputTypeEnum.Api), StringComparison.CurrentCultureIgnoreCase))
                outputTypeEnum |= OutputTypeEnum.Api;
            else if (string.Equals(val, nameof(OutputTypeEnum.Interfaces), StringComparison.CurrentCultureIgnoreCase))
                outputTypeEnum |= OutputTypeEnum.Interfaces;
            else if (string.Equals(val, nameof(OutputTypeEnum.Tests), StringComparison.CurrentCultureIgnoreCase))
                outputTypeEnum |= OutputTypeEnum.Tests;
            throw new InvalidEnumArgumentException($"{outputType} is not a known value");
        }
        return outputTypeEnum;
    }
}
