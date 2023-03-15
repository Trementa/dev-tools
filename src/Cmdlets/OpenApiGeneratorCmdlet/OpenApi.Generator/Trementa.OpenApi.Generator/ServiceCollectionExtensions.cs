using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.DependencyInjection;
using RazorLight.Razor;
using Trementa.OpenApi.Generator.Exceptions;
using Trementa.OpenApi.Generator.Generators.CodeGenerators;
using Trementa.OpenApi.Generator.Generators.TemplateModelEnumerators;
using Trementa.OpenApi.Generator.Razor;
using Trementa.OpenApi.Generator.Tracking;
using Trementa.OpenApi.Generator.Tracking.Observers;

namespace Trementa.OpenApi.Generator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratorServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new Options(configuration);

        services
            .AddSingleton(options)
            .AddTemplateModelEnumerators(options)
            .AddCodeGenerators(options)
            .AddRazorSupport()
            .AddTrackers()
            .AddSingleton<RazorCodeGenerator>()
            .AddTransient<Compiler>()
            .AddHostedService<CodeGeneratorService>();

        return services;
    }

    public static IServiceCollection AddCodeGenerators(this IServiceCollection services, Options options)
    {
        if ((options.OutputType & OutputTypeEnum.Api) != 0)
            services.AddTransient<CodeGenerator, ApiClassCodeGenerator>();
        //if ((options.OutputType & OutputTypeEnum.Model) != 0)
        //    services.AddTransient<CodeGenerator, ModelCodeGenerator>();
        //if ((options.OutputType & OutputTypeEnum.SDK) != 0)
        //    services.AddTransient<CodeGenerator, SDKCodeGenerator>();
        //if (options.DumpTemplate)
        //    services.AddTransient<CodeGenerator, TemplateCodeDumpGenerator>();
        return services;
    }

    public static IServiceCollection AddTemplateModelEnumerators(this IServiceCollection services, Options options)
    {
        if ((options.OutputType & OutputTypeEnum.Api) != 0)
            services.AddTransient<ApiTemplateModelEnumerator>();
        //if ((options.OutputType & OutputTypeEnum.Model) != 0)
        //    services.AddTransient<CodeGenerator, ModelCodeGenerator>();
        //if ((options.OutputType & OutputTypeEnum.SDK) != 0)
        //    services.AddTransient<CodeGenerator, SDKCodeGenerator>();
        //if (options.DumpTemplate)
        //    services.AddTransient<CodeGenerator, TemplateCodeDumpGenerator>();
        return services;
    }


    public static IServiceCollection AddTrackers(this IServiceCollection services)
        => services.AddSingleton<IObserver<FileInfo>, LicenseObserver>()
                   .AddSingleton<IObserver<Error>, ErrorObserver>()
                   .AddSingleton<FileArtifactTracker>()
                   .AddSingleton<ErrorTracker>();

    public static IServiceCollection AddRazorSupport(this IServiceCollection services) =>
        services.AddSingleton<RazorLightProject, RazorTemplatesProject>()
                .AddSingleton<RazorLightEngineBuilder>()
                .AddSingleton<PropertyInjector>()
                .AddSingleton<IEngineHandler, EngineHandler>()
                .AddSingletonCreator((IServiceProvider s, RazorLightProject rcp, RazorLightEngineBuilder rleb) =>
                {
                    IRazorLightEngine razorLightEngine = rleb.UseProject(rcp)
                                                             .UseMemoryCachingProvider()
                                                             .DisableEncoding()
                                                             .Build();
                    AddEngineRenderCallbacks(razorLightEngine, s);
                    return razorLightEngine;
                });

    public static IServiceCollection AddSingletonCreator<TService, T1, T2, T3>(this IServiceCollection services, Func<T1, T2, T3, TService> implementationCreator)
        where TService : class
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        return services.AddSingleton(s =>
        {
            var t1 = s.GetRequiredService<T1>();
            var t2 = s.GetRequiredService<T2>();
            var t3 = s.GetRequiredService<T3>();
            return implementationCreator(t1, t2, t3);
        });
    }

    private static void AddEngineRenderCallbacks(IRazorLightEngine engine, IServiceProvider services)
    {
        var injector = services.GetRequiredService<PropertyInjector>();
        engine.Options.PreRenderCallbacks.Add(delegate (ITemplatePage template) {
            injector.Inject(template);
        });
    }
}
