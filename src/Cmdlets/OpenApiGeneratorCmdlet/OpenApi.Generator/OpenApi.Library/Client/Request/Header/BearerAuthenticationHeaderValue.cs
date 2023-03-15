namespace OpenApi.Library.Client.Request.Header;

public sealed class BearerAuthenticationHeaderValue : IHeaderParameter
{
    readonly string AccessToken;
    public BearerAuthenticationHeaderValue(string accessToken)
        => AccessToken = accessToken;

    public string Header => "Authorization";
    public string Value => $"Bearer {AccessToken}";
}
