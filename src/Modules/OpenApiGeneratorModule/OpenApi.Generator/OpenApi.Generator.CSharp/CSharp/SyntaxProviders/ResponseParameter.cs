namespace OpenApi.Generator.CSharp.SyntaxProviders;

public struct ResponseParameter
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string StatusCode { get; set; }
    public bool IsNullable { get; set; }
    public string MediaType { get; set; }
}
