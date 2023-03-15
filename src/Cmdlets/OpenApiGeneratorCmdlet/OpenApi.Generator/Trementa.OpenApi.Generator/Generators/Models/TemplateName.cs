namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct TemplateName(string Name)
{
    public static implicit operator String(TemplateName name) => name.Name;
}
