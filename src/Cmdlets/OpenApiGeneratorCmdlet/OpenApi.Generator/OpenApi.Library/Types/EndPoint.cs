using System;
using System.Net.Http;

namespace OpenApi.Library.Types
{
    public record EndPoint(Uri baseUri, Uri path)
    {
        public static implicit operator Uri(EndPoint e) => new Uri(e.baseUri, e.path);
    }

    public class UriMethod
    {
        readonly string Method;
        private UriMethod(string method) => Method = method;

        public static implicit operator Uri(UriMethod m) => new Uri(m.Method, UriKind.Relative);
        public static implicit operator UriMethod(string uri) => new UriMethod(uri);
    }

    public record WebCall(HttpMethod Method, Uri baseUri, Uri path)
    {
        public EndPoint EndPoint => new EndPoint(baseUri, path);
    }
}
