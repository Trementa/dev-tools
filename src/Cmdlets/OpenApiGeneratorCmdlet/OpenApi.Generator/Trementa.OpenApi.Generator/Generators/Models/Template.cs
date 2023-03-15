namespace Trementa.OpenApi.Generator.Generators.Models;

public abstract record Template(TemplateName TemplateName, TypeName TypeName);
public abstract record Template<TModel>(TemplateName TemplateType, TypeName TypeName, TModel Model) : Template(TemplateType, TypeName);
