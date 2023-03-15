using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenApi.Library.Extensions;

namespace OpenApi.Library.Extensions;

// https://github.com/skwasjer/Correlate
public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddHttpClient<T>(this IServiceCollection me, IConfiguration configuration)
        where T : class =>
        me.AddHttpClient<T>()
            .CorrelateRequests(configuration["http-client:correlation-key"]);

    public static IServiceCollection AddCorrelate(this IServiceCollection me, IConfiguration configuration) =>
        me.AddCorrelate(options => options.RequestHeaders = new[] { configuration["http-client:correlation-key"] });

    public static IApplicationBuilder UseCorrelate(this IApplicationBuilder me) =>
        Correlate.AspNetCore.AppBuilderExtensions.UseCorrelate(me);
}
