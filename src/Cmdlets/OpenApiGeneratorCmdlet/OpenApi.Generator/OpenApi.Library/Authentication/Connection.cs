#nullable disable

namespace OpenApi.Library.Authentication;

using Client.Request;
using Client.Request.Header;
using Models;

public abstract class Connection : IConnection
{
    public Connection(Configuration configuration)
        => Configuration = configuration;

    internal readonly Configuration Configuration;

    public virtual Uri BaseUri => Configuration.BaseUri;
    public virtual HttpClient HttpClient => new();
    public virtual SecurityToken SecurityToken { get; }

    public virtual async Task<IRequestBuilder> ConfigureRequest(IRequestBuilder requestBuilder, CancellationToken cancellationToken)
    => await requestBuilder
        .AddHeaderParameter(new BearerAuthenticationHeaderValue(SecurityToken.AccessToken))
        .GetAwaiter();
}
