using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Request;
using Types;
public delegate Task<dynamic> HttpRequestFunc(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken);
