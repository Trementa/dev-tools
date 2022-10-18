using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Response;

using Serializer;
using Types;
using Types.Error.Exceptions;
using static Types.Error.ResponseErrorExceptionFactory;

public class ResponseHandler : IResponseHandler
{
    protected readonly ResponseTypeMapper ResponseTypeMapper = new ResponseTypeMapper();
    readonly ISerializer Serializer;

    public ResponseHandler(ISerializer serializer) => Serializer = serializer;

    public void AddResponseMap<T>(string httpStatusCode, string contentType) =>
        ResponseTypeMapper.Add<T>(httpStatusCode, contentType);

    public async Task<dynamic> HandleResponse(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await GetResponse(httpResponseMessage, cancellationToken).ConfigureAwait(false);
            if (httpResponseMessage.IsSuccessStatusCode)
                return response;
            throw Exception(httpResponseMessage, response);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw Exception(httpResponseMessage, httpResponseMessage, innerException: ex);
        }
    }

    public IEnumerable<string> GetAcceptedMediaTypes() =>
        ResponseTypeMapper.GetAcceptedMediaTypes();

    protected virtual async Task<dynamic> GetResponse(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var result = await DeserializeFromStream(response, cancellationToken).ConfigureAwait(false);
        if (result.GetType() == typeof(Void) && response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            dynamic location = response.Headers.Location;
            return location == null ? result : location;
        }
        return result;
    }

    protected async Task<dynamic> DeserializeFromStream(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseType = ResponseTypeMapper.Get(
            (int)response.StatusCode, response.Content.Headers.ContentType?.MediaType);

        if (responseType == typeof(Void))
            return new Void(response.ReasonPhrase);

        if (responseType == typeof(string))
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        if (stream?.CanRead != true)
            return default;

        return await Serializer.DeserializeAsync(stream, responseType, cancellationToken).ConfigureAwait(false);
    }
}
