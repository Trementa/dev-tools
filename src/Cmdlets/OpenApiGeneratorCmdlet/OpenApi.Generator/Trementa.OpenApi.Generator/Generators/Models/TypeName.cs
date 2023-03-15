namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct TypeName(string Name)
{
    public static implicit operator String(TypeName name) => name.Name;
}

public static class StringExtensions
{
    public static string ToTitleCase(this string name)
    {
        var parts = name.Split(new[] { '_', ' ', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(string.Empty, parts.Select(w => char.ToUpper(name[0]) + name[1..]));
    }

    public static string ToCamelCase(this string name)
    {
        var titleCase = ToTitleCase(name);
        return char.ToLower(name[0]) + name[1..];
    }
}