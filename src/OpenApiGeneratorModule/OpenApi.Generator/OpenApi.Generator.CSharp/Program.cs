using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorLight;
using RazorLight.Extensions;

namespace OpenApi.Generator
{
    public static class Program
    {
        public static void Main(params string[] args)
        {
            if (args.Length == 0 || string.Equals(args[0], "help", StringComparison.InvariantCultureIgnoreCase))
                ShowHelp();
            else
                Start(args);
        }

        static void ShowHelp() =>
            Console.Write(Options.Help);

        static void Start(string[] args) =>
            CreateHostBuilder(args).RunConsoleAsync().Wait();

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) =>
                        config.AddCommandLineMapping(args))
                    .UseDefaultServiceProvider((context, options) =>
                        options.ValidateOnBuild = false)
                .ConfigureServices((hostContext, services) =>
                    ConfigureDefaultServices(hostContext.Configuration, services));

        static void ConfigureDefaultServices(IConfiguration configuration, IServiceCollection services)
        {
            var razorCodeProject = new RazorCodeProject(new Options(configuration));
            services.AddRazorLight(() =>
                new RazorLightEngineBuilder()
                    .UseProject(razorCodeProject)
                    .UseMemoryCachingProvider()
                    .DisableEncoding()
                    .Build());
            services.AddSingleton<RazorCodeGenerator>()
                .AddTransient<Compiler>()
                .AddTransient<CSharpCodeGenerator>()
                .AddTransient<CodeGenAdditional>()
                .AddSingleton<Options>()
                .AddSingleton<FileArtifactTracker>()
                .AddSingleton<ErrorTracker>()
                .AddSingleton(razorCodeProject)
                .AddHostedService<CodeGeneratorService>();
        }
    }
}
