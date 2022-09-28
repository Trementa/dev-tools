using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GK.WebLib.Types;

namespace GK.WebLib.Client.Request
{
    public delegate Task<dynamic> HttpRequestFunc(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken);
}
