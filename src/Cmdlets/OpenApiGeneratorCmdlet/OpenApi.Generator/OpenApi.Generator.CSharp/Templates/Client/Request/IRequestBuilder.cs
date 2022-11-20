using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Templates.Client.Request;

public interface IRequestBuilder
{
    IRequestBuilder AddCookieParameter<T>(string name, T value);
    IRequestBuilder AddHeaderParameter<T>(string name, T value);
    IRequestBuilder AddPathParameter<T>(string name, T value);
    IRequestBuilder AddQueryParameter<T>(string name, T value);
    IRequestBuilder AddRequestBody<T>(string name, T content, string contentType);
    IRequestBuilder AddResponseMap<T>(string httpStatusCode, string contentType);
    IRequestBuilder AddResponseMap<T>(string httpStatusCode);
    IRequestBuilder SetRequestBodyMediaType(string requestBodyMediaType);

    Task<HttpRequestMessage> Build(CancellationToken cancellationToken);
    Task<dynamic> HandleResponse(HttpResponseMessage httpResponse, CancellationToken cancellationToken);
    Task<IRequestBuilder> GetAwaiter();
}
