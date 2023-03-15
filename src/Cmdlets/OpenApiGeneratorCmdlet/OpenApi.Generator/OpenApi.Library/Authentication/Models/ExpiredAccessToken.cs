#nullable disable
using System.Text.Json.Serialization;

namespace OpenApi.Library.Authentication.Models;

public record ExpiredAccessToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
