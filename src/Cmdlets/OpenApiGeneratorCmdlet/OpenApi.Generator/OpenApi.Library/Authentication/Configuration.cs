#nullable disable

using OpenApi;

namespace OpenApi.Library.Authentication;

public abstract record Configuration
{
    public virtual Uri BaseUri { get; }
}
