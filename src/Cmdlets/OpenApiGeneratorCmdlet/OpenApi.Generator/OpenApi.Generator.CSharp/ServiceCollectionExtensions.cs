using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenApi.Generator.CSharp.Generators;
using OpenApi.Generator.CSharp.Tracking;
using OpenApi.Generator.CSharp.Tracking.Observers;
using RazorLight;
using RazorLight.DependencyInjection;

namespace OpenApi.Generator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeneratorServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new Options(configuration);

        services
            .AddSingleton(options)
            .AddRazorSupport()
            .AddTrackers()
            .AddSingleton<RazorCodeGenerator>();

            // Not yet fully implemented
            //.AddTransient<ApiTemplateEnumerable>()
            //.AddTransient<ModelTemplateEnumerable>();

        if ((options.OutputType & OutputTypeEnum.Api) != 0)
            services.AddTransient<CodeGenerator, ApiCodeGenerator>();
        if ((options.OutputType & OutputTypeEnum.Model) != 0)
            services.AddTransient<CodeGenerator, ModelCodeGenerator>();
        if ((options.OutputType & OutputTypeEnum.SDK) != 0)
            services.AddTransient<CodeGenerator, SDKCodeGenerator>();
        if (options.DumpTemplate)
            services.AddTransient<CodeGenerator, TemplateCodeDumpGenerator>();

        services
            .AddTransient<Compiler>()
            .AddHostedService<CodeGeneratorService>();

        return services;
    }

    public static IServiceCollection AddTrackers(this IServiceCollection services)
        => services.AddSingleton<IObserver<FileInfo>, LicenseObserver>()
                   .AddSingleton<IObserver<Error>, ErrorObserver>()
                   .AddSingleton<FileArtifactTracker>()
                   .AddSingleton<ErrorTracker>();

    public static IServiceCollection AddRazorSupport(this IServiceCollection services) =>   
        services.AddSingleton<RazorCodeProject>()
                .AddSingleton<RazorLightEngineBuilder>()
                .AddSingleton<PropertyInjector>()
                .AddSingleton<IEngineHandler, EngineHandler>()
                .AddSingletonCreator((IServiceProvider s, RazorCodeProject rcp, RazorLightEngineBuilder rleb) =>
                {
                    IRazorLightEngine razorLightEngine = rleb.UseProject(rcp)
                                                             .UseMemoryCachingProvider()
                                                             .DisableEncoding()
                                                             .Build();
                    AddEngineRenderCallbacks(razorLightEngine, s);
                    return razorLightEngine;
                });

    public static IServiceCollection AddSingletonCreator<TService, T1, T2, T3>(this IServiceCollection services, Func<T1, T2, T3, TService> implementationCreator)
        where TService:class 
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
        PropertyInjector injector = services.GetRequiredService<PropertyInjector>();
        engine.Options.PreRenderCallbacks.Add(delegate (ITemplatePage template)
        {
            injector.Inject(template);
        });
    }
}
