using System;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.DependencyInjection;

namespace OpenApi.Generator;

public static class ServiceCollectionExtensions
{ 
    public static IServiceCollection AddRazorSupport(this IServiceCollection services) =>   
        services.AddSingleton<RazorCodeProject>()
                .AddSingleton<RazorLightEngineBuilder>()
                .AddSingleton<PropertyInjector>()
                .AddSingleton<IEngineHandler, EngineHandler>()
                .AddSingleton(delegate (IServiceProvider s, RazorCodeProject rcp, RazorLightEngineBuilder rleb)
                {
                    IRazorLightEngine razorLightEngine = rleb.UseProject(rcp)
                                                             .UseMemoryCachingProvider()
                                                             .DisableEncoding()
                                                             .Build();
                    AddEngineRenderCallbacks(razorLightEngine, s);
                    return razorLightEngine;
                });

    private static void AddEngineRenderCallbacks(IRazorLightEngine engine, IServiceProvider services)
    {
        PropertyInjector injector = services.GetRequiredService<PropertyInjector>();
        engine.Options.PreRenderCallbacks.Add(delegate (ITemplatePage template)
        {
            injector.Inject(template);
        });
    }
}
