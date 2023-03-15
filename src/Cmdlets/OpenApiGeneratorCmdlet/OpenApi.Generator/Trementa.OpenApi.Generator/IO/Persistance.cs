using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;

namespace Trementa.OpenApi.Generator.IO;
using Generators.Models;
public record Persistance(Logger<Persistance> Logger, Options Options)
{
    public void Save(GeneratedCode generatedCode)
    {

    }



}


