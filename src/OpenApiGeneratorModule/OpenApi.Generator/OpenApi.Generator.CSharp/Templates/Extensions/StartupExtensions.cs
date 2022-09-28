using System;
using GK.WebLib.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GK.WebLib.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection UseStartup<T>(this IServiceCollection serviceCollection, HostBuilderContext hostContext) where T : class
        {
            var startup = ActivatorUtilities.CreateInstance<T>(new HostServiceProvider(hostContext));
            if (startup is IConfigureServices configureServices)
                configureServices.ConfigureServices(serviceCollection, hostContext);
            if (startup is IConfigure configure)
                configure.Configure();
            return serviceCollection;
        }

        private class HostServiceProvider : IServiceProvider
        {
            readonly HostBuilderContext _context;
            public HostServiceProvider(HostBuilderContext context) => _context = context;
            public object GetService(Type serviceType) =>
                serviceType.Name switch {
                    nameof(IHostEnvironment) => _context.HostingEnvironment,
                    nameof(IConfiguration) => _context.Configuration,
                    _ => null
                };
        }

        public static void CreateServiceAndExecute<T>(this IServiceProvider serviceProvider, Action<T> action, params object[] args) where T : class
            => action(ActivatorUtilities.CreateInstance<T>(serviceProvider, args));

        public static void CreateServiceAndExecute<T>(this IHost host, Action<T> action, params object[] args) where T : class
            => CreateServiceAndExecute<T>(host.Services, action, args);
    }
}
