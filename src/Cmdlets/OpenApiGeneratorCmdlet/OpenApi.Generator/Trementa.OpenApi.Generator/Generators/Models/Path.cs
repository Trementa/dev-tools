using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct Path(string OpenApiPath, OpenApiPathItem OpenApiPathItem);
