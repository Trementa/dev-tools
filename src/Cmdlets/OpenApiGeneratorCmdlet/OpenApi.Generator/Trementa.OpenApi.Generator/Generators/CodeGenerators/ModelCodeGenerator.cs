using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Trementa.OpenApi.Generator.Razor;

namespace Trementa.OpenApi.Generator.Generators.CodeGenerators
{
    public class ModelCodeGenerator : CodeGenerator
    {
        protected readonly RazorCodeGenerator RazorCodeGenerator;
        //protected readonly ModelTemplateEnumerable ModelTemplateEnumerable;
        protected readonly Logger Logger;

        public ModelCodeGenerator(RazorCodeGenerator razorCodeGenerator, /*ModelTemplateEnumerable modelTemplateEnumerable,*/ Logger<ModelCodeGenerator> logger)
        {
            RazorCodeGenerator = razorCodeGenerator;
            //ModelTemplateEnumerable = modelTemplateEnumerable;
            Logger = logger;
        }

        public override async Task Generate(OpenApiDocument doc, CancellationToken cancellationToken = default)
        {
            await GenerateModels(doc.Components, cancellationToken);
        }

        protected async Task GenerateModels(OpenApiComponents components, CancellationToken cancellationToken)
        {
            foreach (var model in components.Schemas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var modelName = model.Key.ToPascalCase();
                //var fileName = CreateFileName("Model", modelName, ".cs");
                //await RazorCodeGenerator.GenerateAndSaveCode("Model", fileName,
                //    (modelName, model.Value));
            }
        }
    }
}
