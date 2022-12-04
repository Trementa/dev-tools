using System;
using System.IO;

namespace OpenApi.Generator.Test
{
    public static class Program
    {
        public static void Main()
        {
            Generator.Program.Main(
               "-o", @"D:/temp/dRofus.Api.Proxy",
               "-s", @"https://api-no.drofus.com/swagger/v1/swagger.json",
               "--GenNamespace", "dRofus.Api.Proxy",
               "--SDKNamespace", "dRofus.WebLib",
               "-c", "yes",
               "-f", "yes",
               "-g", "yes",
               "-l", "C#",
               "--outputType", "Api|Model|SDK",
               "--excludeAPIParams", "db, pr",
               "--useHTTPverbs");
        }

        static string GetProjectPath(string projectName, DirectoryInfo dirInfo)
        {
            if (dirInfo == null)
                throw new Exception($"{projectName} not found");
            var directories = dirInfo.GetDirectories(projectName, SearchOption.TopDirectoryOnly);
            if (directories.Length == 1)
            {
                var files = directories[0].GetFiles($"{projectName}.csproj", SearchOption.TopDirectoryOnly);
                if (files.Length == 1)
                    return files[0].DirectoryName;
            }

            return GetProjectPath(projectName, dirInfo.Parent);
        }
    }
}
