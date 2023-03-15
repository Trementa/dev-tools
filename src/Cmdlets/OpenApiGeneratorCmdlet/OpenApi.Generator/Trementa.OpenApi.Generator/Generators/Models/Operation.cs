using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct Operation(OperationType OperationType, OpenApiOperation OpenApiOperation);
