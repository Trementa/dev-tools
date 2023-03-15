using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace OpenApi.Library.Client.Request;

using Header;
using OpenApi.Library.Client.Response;
using Response;
using Serializer;
using Types;

public static partial class Extensions
{
    public static TInstance AddAsString<TValue, TInstance>(this Dictionary<string, string> dictionary, string key, TValue value, TInstance instance)
    {
        if (value != null)
            dictionary.TryAdd(key, FormattableString.Invariant($"{value}"));
        return instance;
    }

    public static TInstance AddAsString<TValue, TInstance>(this MultiValueDictionary<string, string> dictionary, string key, TValue value, TInstance instance)
    {
        if (value != null)
            dictionary.Add(key, FormattableString.Invariant($"{value}"));
        return instance;
    }
}

public class RequestBuilder : IRequestBuilder
{
    public static RequestBuilder Create(HttpMethod httpMethod, Uri uri) =>
        new RequestBuilder(httpMethod, uri);
    public static RequestBuilder Create(WebCall webCall) =>
        new RequestBuilder(webCall.Method, webCall.EndPoint);
    public static RequestBuilder Create(HttpMethod httpMethod, Uri baseUri, Uri relativeUri) =>
        Create(httpMethod, new EndPoint(baseUri, relativeUri));

    protected readonly Dictionary<string, (string MediaType, dynamic Content)> Content =
        new Dictionary<string, (string, dynamic)>();

    protected readonly Dictionary<string, string> CookieParameters = new Dictionary<string, string>();

    protected readonly MultiValueDictionary<string, string> HeaderParameters =
        new MultiValueDictionary<string, string>();

    protected readonly Dictionary<string, string> PathParameters = new Dictionary<string, string>();
    protected readonly Dictionary<string, string> QueryParameters = new Dictionary<string, string>();
    protected readonly HttpMethod HttpMethod;
    protected readonly IResponseHandler ResponseHandler;
    protected readonly ISerializer Serializer;
    protected readonly Uri Path;
    protected string RequestBodyMediaType { get; set; }

    protected internal RequestBuilder(HttpMethod httpMethod, Uri path)
    {
        HttpMethod = httpMethod;
        Path = path;
        Serializer = new JsonSerializer();
        ResponseHandler = new ResponseHandler(Serializer);
    }

    public IRequestBuilder AddCookieParameter<T>(string name, T value) =>
        CookieParameters.AddAsString(name, value, this);

    public IRequestBuilder AddHeaderParameter<T>(string name, T value) =>
        HeaderParameters.AddAsString(name, value, this);
    public IRequestBuilder AddHeaderParameter(IHeaderParameter headerParameter) =>
        HeaderParameters.AddAsString(headerParameter.Header, headerParameter.Value, this);

    public IRequestBuilder AddPathParameter<T>(string name, T value) =>
        PathParameters.AddAsString(name, value, this);

    public IRequestBuilder AddQueryParameter<T>(string name, T value) =>
        QueryParameters.AddAsString(name, value, this);

    public IRequestBuilder AddRequestBody<T>(string name, T content, string contentType)
    {
        Content.TryAdd(name, (contentType, content));
        return this;
    }

    public IRequestBuilder AddResponseMap<T>(string httpStatusCode, string contentType)
    {
        ResponseHandler.AddResponseMap<T>(httpStatusCode, contentType);
        return this;
    }

    public IRequestBuilder AddResponseMap<T>(string httpStatusCode)
    {
        ResponseHandler.AddResponseMap<T>(httpStatusCode, "*/*");
        return this;
    }

    public IRequestBuilder SetRequestBodyMediaType(string requestBodyMediaType)
    {
        RequestBodyMediaType = requestBodyMediaType;
        return this;
    }

    public async Task<HttpRequestMessage> Build(CancellationToken cancellationToken)
    {
        var httpRequestMessage = new HttpRequestMessage(HttpMethod, CreateUri());
        AddHeadersToRequest(httpRequestMessage);
        PutCookiesOnRequest(httpRequestMessage);
        var acceptedMediaTypes = ResponseHandler.GetAcceptedMediaTypes();
        httpRequestMessage.Headers.Add("Accept", acceptedMediaTypes);
        httpRequestMessage.Content = await CreateHttpContent(cancellationToken).ConfigureAwait(false);
        return httpRequestMessage;
    }

    public async Task<dynamic> HandleResponse(HttpResponseMessage httpResponse, CancellationToken cancellationToken) =>
        await ResponseHandler.HandleResponse(httpResponse, cancellationToken).ConfigureAwait(false);

    public async Task<IRequestBuilder> GetAwaiter() => await Task.FromResult(this as IRequestBuilder).ConfigureAwait(false);

    protected Uri CreateUri()
    {
        var builder = new UriBuilder(BindPathParameters()) { Port = -1 };
        var query = HttpUtility.ParseQueryString(builder.Query);
        foreach (var queryParam in QueryParameters)
            query[queryParam.Key] = queryParam.Value;
        builder.Query = query.ToString();
        return builder.Uri;
    }

    protected string BindPathParameters()
    {
        var strB = new StringBuilder(Path.OriginalString);
        foreach (var pathParameter in PathParameters)
            strB.Replace($"{{{pathParameter.Key}}}", pathParameter.Value);

        return strB.ToString();
    }

    protected void AddHeadersToRequest(HttpRequestMessage httpRequestMessage)
    {
        foreach (var header in HeaderParameters)
            httpRequestMessage.Headers.Add(header.Key, header.Value);
    }

    protected void PutCookiesOnRequest(HttpRequestMessage request)
    {
        var cookie = FormatCookieString();
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Headers.Add("Cookie", FormatCookieString());
    }

    protected string FormatCookieString() =>
        string.Join("; ", CookieParameters.Select(c => $"{c.Key}={c.Value}"));

    protected async Task<HttpContent> CreateHttpContent(CancellationToken cancellationToken)
    {
        if (RequestBodyMediaType == "multipart/form-data" || Content.Count > 1)
        {
            var httpContent = new MultipartFormDataContent();
            foreach (var content in Content)
            {
                var subContent = await GetHttpContent(content.Value.MediaType, content.Value.Content,
                    cancellationToken);
                subContent.Headers.ContentDisposition ??= new ContentDispositionHeaderValue("form-data");
                subContent.Headers.ContentDisposition.Name = content.Key;
                httpContent.Add(subContent);
            }

            return httpContent;
        }
        else
        {
            var content = Content.FirstOrDefault();
            if (content.Key == default)
                return null;
            var httpContent =
                await GetHttpContent(content.Value.MediaType, content.Value.Content, cancellationToken).ConfigureAwait(false);
            httpContent.Headers.ContentDisposition = null;
            return httpContent;
        }
    }

    protected async Task<HttpContent> GetHttpContent(string mediaType, dynamic content,
        CancellationToken cancellationToken)
    {
        var httpContent = await SerializeIntoStream(content, cancellationToken).ConfigureAwait(false);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        return httpContent;
    }

    protected async Task<HttpContent> SerializeIntoStream(FileStream file,
        CancellationToken cancellationToken)
    {
        var httpContent = new StreamContent(file);
        httpContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
            FileName = System.IO.Path.GetFileName(file.Name)
        };
        return await Task.FromResult(httpContent).ConfigureAwait(false);
    }

    protected async Task<HttpContent> SerializeIntoStream(string content,
        CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        var stringWriter = new StreamWriter(ms);
        await stringWriter.WriteAsync(content).ConfigureAwait(false);
        await stringWriter.FlushAsync().ConfigureAwait(false);
        ms.Seek(0, SeekOrigin.Begin);
        return new StreamContent(ms);
    }

    protected async Task<HttpContent> SerializeIntoStream(IDictionary<string, string> content,
        CancellationToken cancellationToken) =>
        await Task.FromResult(new FormUrlEncodedContent(content)).ConfigureAwait(false);

    protected async Task<HttpContent> SerializeIntoStream(object content,
        CancellationToken cancellationToken)
    {
        var ms = new MemoryStream();
        await Serializer.SerializeAsync(ms, content, cancellationToken).ConfigureAwait(false);
        ms.Seek(0, SeekOrigin.Begin);
        return new StreamContent(ms);
    }

    public override string ToString()
    {
        var strB = new StringBuilder();
        strB.AppendLine($"Request URL: {CreateUri()}")
            .AppendLine($"Request Method: {HttpMethod}");

        foreach (var header in HeaderParameters)
            strB.AppendLine($"{header.Key}: {string.Join(';', header.Value.ToArray())}");

        var cookie = FormatCookieString();
        if (!string.IsNullOrWhiteSpace(cookie))
            strB.AppendLine($"Cookie: {cookie}");

        var acceptedMediaTypes = ResponseHandler.GetAcceptedMediaTypes();
        strB.AppendLine($"Accept: {string.Join(';', acceptedMediaTypes.ToArray())}");

        var body = Content.FirstOrDefault();
        if (body.Value.Content != null)
            strB.AppendLine($"Content: {Serializer.Serialize(body.Value.Content)}");

        return strB.ToString();
    }
}
