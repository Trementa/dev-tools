namespace Trementa.OpenApi.Generator.CSharp.SyntaxProviders;

public struct FunctionArgument
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public bool IsNullable { get; set; }
    public bool IsRequired { get; set; }
    public string Description { get; set; }

    public enum ArgumentLocation { Query, Header, Path, Cookie, Request, Unknown };

    public ArgumentLocation In { get; set; }

    public override string ToString()
    {
        if (IsNullable)
            return $"{Type}? {Name}{(IsRequired ? "" : " = default")}";
        return $"{Type} {Name}{(IsRequired ? "" : " = default")}";
    }
}
