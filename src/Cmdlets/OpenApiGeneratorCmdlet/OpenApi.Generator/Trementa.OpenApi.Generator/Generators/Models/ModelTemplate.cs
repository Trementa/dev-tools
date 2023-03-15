namespace Trementa.OpenApi.Generator.Generators.Models;

public record ModelTemplate<TModel>(TypeName TypeName, TModel Model) : Template<TModel>(new TemplateName("Model"), TypeName, Model);
