using Microsoft.OpenApi.Models;
using Trementa.OpenApi.Generator.Generators.TemplateModelEnumerators;
using Trementa.OpenApi.Generator.Razor;

namespace Trementa.OpenApi.Generator.Generators.CodeGenerators;

public class ApiClassCodeGenerator : CodeGenerator
{
    protected readonly RazorCodeGenerator RazorCodeGenerator;
    protected readonly ApiTemplateModelEnumerator ApiTemplateEnumerable;
    protected readonly Logger Logger;

    public ApiClassCodeGenerator(RazorCodeGenerator razorCodeGenerator, ApiTemplateModelEnumerator apiTemplateModelEnumerator, Logger<ApiClassCodeGenerator> logger)
    {
        RazorCodeGenerator = razorCodeGenerator;
        ApiTemplateEnumerable = apiTemplateModelEnumerator;
        Logger = logger;
    }

    public override async Task Generate(OpenApiDocument document, CancellationToken cancellationToken = default)
    {
        foreach (var apiTemplateModel in ApiTemplateEnumerable.EnumerateApiByTag(document))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileName = CreateFileName(apiTemplateModel.TemplateName, apiTemplateModel.TypeName, ".cs");
             await RazorCodeGenerator.GenerateAndSaveCode(apiTemplateModel, fileName);
        }
    }
}
