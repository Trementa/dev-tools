using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.Models;

public record ApiTemplateModel(TypeName TypeName, IEnumerable<(string Key, OpenApiPathItem Value)> Model) : Template(new TemplateName("Api2"), TypeName);
