using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenApi.Library.Configurations;

public interface IConfigureServices
{
    IServiceCollection ConfigureServices();
}