using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct Class
{
    public Class(OpenApiPathItem OpenPathItem, IEnumerable<Operation> Operations)
        => (this.OpenApiPathItem, this.Operations) = (OpenPathItem, Operations);

    public readonly OpenApiPathItem OpenApiPathItem;
    public IEnumerable<Operation> Operations { get; }
}
