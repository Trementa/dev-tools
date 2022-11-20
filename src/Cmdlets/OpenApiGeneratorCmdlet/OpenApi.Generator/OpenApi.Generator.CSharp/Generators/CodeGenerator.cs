using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace OpenApi.Generator.CSharp.Generators
{
    public abstract class CodeGenerator
    {
        public abstract Task Generate(OpenApiDocument document, CancellationToken cancellationToken = default);

        protected string CreateFileName(string folder, string typeName, string postfix) =>
            Path.Combine(folder, string.Concat(typeName, postfix));
    }
}
