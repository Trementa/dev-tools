using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Response;

public interface IResponseHandler
{
    void AddResponseMap<T>(string httpStatusCode, string contentType);
    IEnumerable<string> GetAcceptedMediaTypes();
    Task<dynamic> HandleResponse(HttpResponseMessage response, CancellationToken cancellationToken);
}
