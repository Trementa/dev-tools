using System;
using System.Threading;
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

        static async void Start(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync().WaitAsync(CancellationToken.None);
            //Console.WriteLine("Press any key");
            //Console.ReadKey();
        }

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
            services
                .AddTransient(typeof(Logger<>))
                .AddSingleton<RazorCodeGenerator>()
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
