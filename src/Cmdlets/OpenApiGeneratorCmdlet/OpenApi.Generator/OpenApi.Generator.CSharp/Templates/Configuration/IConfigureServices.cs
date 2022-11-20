using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Templates.Configuration;

public interface IConfigureServices
{
    void ConfigureServices(IServiceCollection services, HostBuilderContext context);
}