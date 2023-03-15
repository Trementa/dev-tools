#nullable disable 

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenApi.Library.Extensions;

using Configurations;
using OpenApi.Library.Types.Exceptions;

public static class StartupExtensions
{
    public static IServiceCollection UseStartup<T>(this IServiceCollection serviceCollection, HostBuilderContext hostContext) where T : IConfigureServices
    {
        var startup = ActivatorUtilities.CreateInstance<T>(new HostServiceProvider(hostContext));
        return startup.ConfigureServices();
    }

    private class HostServiceProvider : IServiceProvider
    {
        readonly HostBuilderContext context;
        public HostServiceProvider(HostBuilderContext context) => this.context = context;
        public object GetService(Type serviceType) =>
            serviceType.Name switch {
                nameof(IHostEnvironment) => context.HostingEnvironment,
                nameof(IConfiguration) => context.Configuration,
                _ => throw new UnknownTypeException(serviceType)
            };
    }
}
