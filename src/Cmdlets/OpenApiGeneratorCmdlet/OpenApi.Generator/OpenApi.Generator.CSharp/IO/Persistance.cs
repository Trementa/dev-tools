using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpYaml.Tokens;

namespace OpenApi.Generator.CSharp.IO;
public record Persistance(Logger<Persistance> Logger, Options Options)
{
    public void Save(GeneratedCode generatedCode)
    {

    }



}


public record struct TemplateName(string Name);
public record struct TypeName(string Name);

public abstract record Template(TemplateName TemplateType, TypeName TypeName);
public abstract record Template<TModel>(TemplateName TemplateType, TypeName TypeName, TModel Model) : Template(TemplateType, TypeName);

public record ApiTemplate<TModel>(TypeName TypeName, TModel Model) : Template<TModel>(new TemplateName("Api"), TypeName, Model);
public record ModelTemplate<TModel>(TypeName TypeName, TModel Model) : Template<TModel>(new TemplateName("Model"), TypeName, Model);

public record struct GeneratedCode(string Code);

//public record struct TemplateFile(string Path);

//public record struct GeneratedEntity(GeneratedCode Code, )
