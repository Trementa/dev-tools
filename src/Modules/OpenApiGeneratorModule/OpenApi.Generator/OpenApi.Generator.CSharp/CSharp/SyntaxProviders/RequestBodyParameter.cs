namespace OpenApi.Generator.CSharp.SyntaxProviders;

public struct RequestBodyParameter
{
    public string Type { get; set; }
    public string Name { get; set; }
    public bool IsNullable { get; set; }
    public string MediaType { get; set; }
}
