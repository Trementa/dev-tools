#nullable disable
using System.Text.Json.Serialization;

namespace OpenApi.Library.Authentication.Models;

public record SecurityToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
}
