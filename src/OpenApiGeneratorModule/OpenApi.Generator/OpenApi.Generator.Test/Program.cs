using System;
using System.IO;

namespace OpenApi.Generator.Test
{
    public static class Program
    {
        public static void Main()
        {
            Generator.Program.Main(
               "-o", @"D:\Source\gkkg-root\gkkg-tools\OpenApi.Generator\Test",
               "-s", @"https://api.gkkg.no/swagger/v1/swagger.json",
               "--GenNamespace", "GK.Api.Proxy",
               "--SDKNamespace", "GK.WebLib",
               "-c", "yes",
               "-f", "yes",
               "-g", "yes",
               "-l", "C#",
               "--outputType", "Api|Model|SDK");
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
