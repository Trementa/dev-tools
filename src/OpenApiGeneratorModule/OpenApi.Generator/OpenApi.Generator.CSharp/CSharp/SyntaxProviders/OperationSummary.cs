using System;
using System.Collections.Generic;
using System.Text;

namespace OpenApi.Generator.CSharp.SyntaxProviders;
#nullable enable

public struct OperationSummary
{
    public string? Summary { get; set; }
    public IEnumerable<FunctionArgument> Arguments { get; set; }
    public string ReturnType { get; set; }

    public string CreateComment(string indentation)
    {
        var strB = new StringBuilder();
        indentation = $"{indentation}/// ";

        AppendSummary(indentation, strB);
        AppendArgumentsDescription(indentation, strB);
        AppendReturnTypeDescription(indentation, strB);

        return strB.ToString();
    }

    private void AppendSummary(string indentation, StringBuilder strB)
        => strB.AppendLine(CreateDescription(indentation, "<summary>", "</summary", Summary!));

    private void AppendArgumentsDescription(string indentation, StringBuilder strB)
    {
        foreach (var argument in Arguments)
            strB.AppendLine(CreateDescription(indentation, $@"<param name=""{argument.Name}"">", "</param>", argument.Description));
    }

    private void AppendReturnTypeDescription(string indentation, StringBuilder strB)
        => strB.Append(CreateDescription(indentation, "<returns>", "</returns>", ReturnType));

    private string CreateDescription(string indentation, string startTag, string endTag, string text)
        => $"{indentation}{startTag}{CreateComment(indentation, text, endTag)}";

    private string CreateComment(string indentation, string text, string endTag)
    {
        if (null == text)
            return string.Empty;

        var lines = text.Split('\n');
        if (lines.Length == 1)
            return $"{lines[0]}{endTag}";

        var prefix = $"{Environment.NewLine}{indentation}";
        return $"{Environment.NewLine}{indentation}{string.Join(indentation, lines)}{Environment.NewLine}{indentation}{endTag}";
    }
}
