using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GK.WebLib.Configuration;

public interface IConfigureServices
{
    void ConfigureServices(IServiceCollection services, HostBuilderContext context);
}