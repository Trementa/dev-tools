using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace OpenApi.Generator
{
    public class Options
    {
        public static readonly Dictionary<string, string> SwitchMappings = new Dictionary<string, string>
                    {
                        { "-a", "addtraceid" },
                        { "--addtraceid", "addtraceid" },
                        { "-c", "continueOnError" },
                        { "--continue", "continueOnError" },
                        { "-d", "dumpTemplate" },
                        { "--dump", "dumpTemplate" },
                        { "--excludeAPIParams", "excludeAPIParams "},
                        { "-f", "force" },
                        { "--force", "force" },
                        { "-g", "generate" },
                        { "--generate", "generate" },
                        { "-l", "templateLanguage" },
                        { "--lang", "templateLanguage" },
                        { "-n", "namespace" },
                        { "--namespace", "namespace" },
                        { "-o", "outputDirectory" },
                        { "--output", "outputDirectory" },
                        { "--outputType", "OutputType" },
                        { "-s", "source" },
                        { "--source", "source" },
                        { "-t", "templateDirectory" },
                        { "--templates", "templateDirectory" },
                        { "--useHTTPverbs", "useHTTPverbs" },
                        { "-q", "quiet" },
                        { "--quiet", "quiet" }
                    };

        public static readonly string Help = $@"{ assembly.FullName } v{ assembly.GetName().Version }
help           This help text

Parameters
--addtraceid       -a       Adds optional x-trace-id header to all apis
--convertToV3,     -3 [Uri] Convert definition to V3 if less
--continueOnError, -c       Continue executing after an error occurs
--dump,            -d       Dump templates
--excludeAPIParams          Comma separated list of arguments to ignore when generating API methods
--force,           -f       Overwrite existing files
--generate,        -g       Generate code, default template is C#
--lang,            -l       Language template
--namespace,       -n @Ns   Namespace to use for output
--output,          -o Dir   Output directory
--source,          -s Uri   Open Api source definition (v3+)
--templates,       -t Dir   Template directory
--outputType                All|[SDK|Api|Model]
--quiet,           -q       Don't show any log information
--useHTTPVerbs              Use HTTP method names in API methods";
        static Assembly assembly => Assembly.GetExecutingAssembly();
        protected readonly IConfiguration Configuration;

        public Options(IConfiguration configuration)
        {
            Configuration = configuration;

            if (string.IsNullOrWhiteSpace(SourceFile))
                throw new ArgumentException("Need to specify source");

            if (string.IsNullOrWhiteSpace(OutputDirectory))
                OutputDirectory = Directory.GetCurrentDirectory();

            if (!Force && !IsDirectoryEmpty(OutputDirectory))
                throw new ArgumentException(
                    $"{OutputDirectory} is not empty. Use -f or --force to disregard.");

            if (string.IsNullOrWhiteSpace(SDKNamespace))
                SDKNamespace = "GKCloud.Integrations.General.";

            if (string.IsNullOrWhiteSpace(GenNamespace))
                GenNamespace = "OpenApi.Generated";

            if (!SDKNamespace.EndsWith('.'))
                SDKNamespace = $"{SDKNamespace}.";

            if (!GenNamespace.EndsWith('.'))
                GenNamespace = $"{GenNamespace}.";

            TemplateDirectory ??= $"{System.AppContext.BaseDirectory}Templates";

            bool IsDirectoryEmpty(string path) =>
                !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public string CsTemplate => ".cst";
        public string CsCompiled => ".cs";
        public string CsProject => ".csproj";

        public bool Generate => !string.IsNullOrWhiteSpace(Configuration["generate"]);
        public bool DumpTemplate => !string.IsNullOrWhiteSpace(Configuration["dumpTemplate"]);

        public string TemplateLanguage => Configuration["templateLanguage"] ?? "C#";
        public string SourceFile => Configuration["source"];

        public string OutputDirectory
        {
            get => Configuration["outputDirectory"];
            protected set => Configuration["outputDirectory"] = value;
        }

        public string SDKNamespace
        {
            get => Configuration["SDKNamespace"];
            protected set => Configuration["SDKNamespace"] = value;
        }

        public string GenNamespace
        {
            get => Configuration["GenNamespace"];
            protected set => Configuration["GenNamespace"] = value;
        }

        public string TemplateDirectory
        {
            get => Configuration["templateDirectory"];
            protected set => Configuration["templateDirectory"] = value;
        }

        public bool Force => !string.IsNullOrWhiteSpace(Configuration["force"]);
        public bool IncludeTraceId => !string.IsNullOrWhiteSpace(Configuration["addtraceid"]);

        public bool Quiet => !string.IsNullOrWhiteSpace(Configuration["quiet"]);

        public bool ContinueOnError => !string.IsNullOrWhiteSpace(Configuration["continueOnError"]);

        public bool UseHTTPverbs => !string.IsNullOrWhiteSpace(Configuration["generate"]);

        public IEnumerable<string> ExcludeAPIParams => Configuration["excludeAPIParams"].Split(',').Select(s => s.Trim());

        [Flags]
        public enum OutputTypeEnum
        {
            All = 7,
            SDK = 1,
            Model = 2,
            Api = 4,
            Gen = 6
        };

        public OutputTypeEnum OutputType
        {
            get
            {
                var outputType = Configuration["OutputType"];

                if (string.IsNullOrWhiteSpace(outputType))
                    return OutputTypeEnum.All;

                var outputTypeEnum = (OutputTypeEnum)0;
                var splits = outputType.Split('|');
                foreach (var split in splits)
                {
                    var val = split.Trim();
                    if (string.Equals(val, nameof(OutputTypeEnum.All), StringComparison.CurrentCultureIgnoreCase))
                        outputTypeEnum |= OutputTypeEnum.All;
                    else if (string.Equals(val, nameof(OutputTypeEnum.SDK), StringComparison.CurrentCultureIgnoreCase))
                        outputTypeEnum |= OutputTypeEnum.SDK;
                    else if (string.Equals(val, nameof(OutputTypeEnum.Gen), StringComparison.CurrentCultureIgnoreCase))
                        outputTypeEnum |= OutputTypeEnum.Gen;
                    else if (string.Equals(val, nameof(OutputTypeEnum.Model), StringComparison.CurrentCultureIgnoreCase))
                        outputTypeEnum |= OutputTypeEnum.Model;
                    else if (string.Equals(val, nameof(OutputTypeEnum.Api), StringComparison.CurrentCultureIgnoreCase))
                        outputTypeEnum |= OutputTypeEnum.Api;
                    else
                        throw new InvalidEnumArgumentException($"{outputType} is not a known value");
                }
                return outputTypeEnum;
            }
        }

        public override string ToString() =>
            Configuration.PrettyPrint();
    }
}