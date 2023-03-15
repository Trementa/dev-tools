using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Trementa.OpenApi.Generator.CSharp.Utility;

public static class CSharpIdentifiers
{
    private static HashSet<string> _keywords = new HashSet<string> {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "partial", "private", "protected", "public", "readonly", "record", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
        "virtual", "void", "volatile", "while"
    };

    public static string CreateValidIdentifier(string identifier, char defaultReplacementCharacter = '_', params (char Match, string Replacement)[] matchAndReplaceMap)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return "null";
        if (_keywords.Contains(identifier)) return $"@{identifier}";

        var sb = new StringBuilder(identifier.Length);
        for (var i = 0; i < identifier.Length; i++)
        {
            var c = identifier[i];

            if (TryGetMappedCharacter(matchAndReplaceMap, c, out var replacement))
                sb.Append(replacement);
            else if (i == 0 && c == '@')
                sb.Append(c);
            else if (char.IsAscii(c) && char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append(defaultReplacementCharacter);
        }

        return sb.ToString();
    }

    static bool TryGetMappedCharacter((char Match, string Replacement)[] matchAndReplaceMap, char c, out string replacement)
    {
        var map = matchAndReplaceMap.FirstOrDefault(map => map.Match == c);
        if (map.Match == c)
        {
            replacement = map.Replacement;
            return true;
        }

        replacement = null;
        return false;
    }

    public static bool IsValidParsedIdentifier(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;

        if (!IsValidParsedIdentifierStart(str, 0)) return false;

        var firstCharWidth = char.IsSurrogatePair(str, 0) ? 2 : 1;

        for (var i = firstCharWidth; i < str.Length;) //Manual increment
        {
            if (!IsValidParsedIdentifierPart(str, i)) return false;
            if (char.IsSurrogatePair(str, i)) i += 2;
            else i += 1;
        }

        return true;
    }

    //(String, index) pairs are used instead of chars in order to support surrogate pairs
    //(Unicode code-points above 2^16 represented using two 16-bit characters)

    public static bool IsValidParsedIdentifierStart(string s, int index)
    {
        return s[index] == '_' || char.IsLetter(s, index) || char.GetUnicodeCategory(s, index) == UnicodeCategory.LetterNumber;
    }

    public static bool IsValidParsedIdentifierPart(string s, int index)
    {
        if (s[index] == '_' || s[index] >= '0' && s[index] <= '9' || char.IsLetter(s, index)) return true;

        switch (char.GetUnicodeCategory(s, index))
        {
            case UnicodeCategory.LetterNumber: //Eg. Special Roman numeral characters (not covered by IsLetter())
            case UnicodeCategory.DecimalDigitNumber: //Includes decimal digits in other cultures
            case UnicodeCategory.ConnectorPunctuation:
            case UnicodeCategory.NonSpacingMark:
            case UnicodeCategory.SpacingCombiningMark:
                //UnicodeCategory.Format handled in CreateValidIdentifier()
                return true;
            default:
                return false;
        }
    }
}
