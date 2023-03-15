using System;
using System.Text;

namespace OpenApi.Library.Client.Request.Header;

public sealed class BasicAuthenticationHeaderValue : IHeaderParameter
{
    readonly string BasicCredentials;
    public BasicAuthenticationHeaderValue(string username, string password)
    {
        var basicUserPasswordPair = $"{username}:{password}";
        var basicBinaryCredential = Encoding.ASCII.GetBytes(basicUserPasswordPair);
        BasicCredentials = Convert.ToBase64String(basicBinaryCredential);
    }

    public string Header => "Authorization";
    public string Value => $"Basic {BasicCredentials}";
}
