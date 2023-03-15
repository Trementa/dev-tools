using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Library.Client.Request;

using OpenApi.Library.Types;
using Types;
public delegate Task<dynamic> HttpRequestFunc(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken);
