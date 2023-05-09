using System;
using System.IO;

namespace OpenApi.Generator.Test;
public static class Program
{
    public static void Main()
    {
        var projectContainer = new ProjectContainer("OpenApi.Generator.Test_DoNotUseNow", "D:\\temp\\A_TEST.OpenApi.Generator.Test");

        Trementa.OpenApi.Generator.Program.Main(
           "-o", projectContainer.ProjectPath,
           //"-o", @"D:/temp/HydroGrid",
           //"-s", @"D:\Source\Trementa\dev-tools\src\Cmdlets\OpenApiGeneratorCmdlet\OpenApi.Generator\OpenApi.Generator.Test\TestFileDefinitions\HYDROGRID-Insight-API_OpenAPI-MERGED_EXTERNALS.yaml",
           //"-s", @"D:\Source\Trementa\dev-tools\src\Cmdlets\OpenApiGeneratorCmdlet\OpenApi.Generator\OpenApi.Generator.Test\TestFileDefinitions\swaggerTripletex20.json",
           "-s", @"https://api-spec.hydrogrid.eu/swagger.yaml",
           "-ta", @"Trementa.OpenApi.Templates.CSharp.dll",
           "--GenNamespace", "HydroGrid.Proxy",
           "--SDKNamespace", "Trementa.WebLib",
           "-c", "yes",
           "-f", "yes",
           "-g", "yes",
           "-l", "C#",
           "--outputType", "Api|Model|SDK",
           //"--excludeAPIParams", "db, pr",
           "--useHTTPverbs");
    }
}
