using Microsoft.OpenApi.Models;
using Trementa.OpenApi.Generator.Generators.Models;

namespace Trementa.OpenApi.Generator.CSharp.SyntaxProviders;
public enum ArgumentLocation { Query, Header, Path, Cookie, Request, Body, Unknown };

public record FunctionArgument
{
    public OpenApiSchema Schema { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public bool IsNullable { get; set; }
    public bool IsRequired { get; set; }
    public string Description { get; set; }


    public virtual ArgumentLocation In { get; set; }

    public override string ToString()
    {
        if (IsNullable)
            return $"{Type}? {Name}{(IsRequired ? "" : " = default")}";
        return $"{Type} {Name}{(IsRequired ? "" : " = default")}";
    }
}

public record RequestBodyParameter : FunctionArgument
{
    public string MediaType { get; set; }
    public override ArgumentLocation In => ArgumentLocation.Body;
}

