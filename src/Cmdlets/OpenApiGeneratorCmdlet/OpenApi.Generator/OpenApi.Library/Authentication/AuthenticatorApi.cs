using Microsoft.Extensions.Logging;

namespace OpenApi.Library.Authentication;
using Api;
using Client.Request;

public abstract class AuthenticatorApi : BaseApi<AuthenticatorApi>
{
    public AuthenticatorApi(IConnection connection, ILogger<AuthenticatorApi> logger) : base(connection, logger)
    { }

    protected override Task<IRequestBuilder> ConfigureRequest(RequestBuilder requestBuilder, CancellationToken cancellationToken)
        => requestBuilder.GetAwaiter();
}
