using System.Collections.Generic;
using System.Net;

namespace Templates.Types
{
    public static class StatusCodes
    {
        static readonly IDictionary<int, StatusCode> statusCodes = new Dictionary<int, StatusCode>();

        // Unknown status code
        static readonly StatusCode NotAKnownStatusCode = new StatusCode(-1, nameof(NotAKnownStatusCode));

        // Informational status codes
        public static readonly StatusCode Informational = New(100, nameof(Informational)); // Do not change order
        public static readonly StatusCode Continue = New(100, nameof(Continue));           // between these two lines
        public static readonly StatusCode SwitchingProtocol = New(101, nameof(SwitchingProtocol));
        public static readonly StatusCode ProcessingWebDav = New(102, nameof(ProcessingWebDav));
        public static readonly StatusCode EarlyHints = New(103, nameof(EarlyHints));

        // Success status codes
        public static readonly StatusCode Success = New(200, nameof(Success)); // Do not change order
        public static readonly StatusCode Ok = New(200, nameof(Ok));           // between these two lines
        public static readonly StatusCode Created = New(201, nameof(Created));
        public static readonly StatusCode Accepted = New(202, nameof(Accepted));
        public static readonly StatusCode NonAuthoritativeInformation = New(203, nameof(NonAuthoritativeInformation));
        public static readonly StatusCode NoContent = New(204, nameof(NoContent));
        public static readonly StatusCode ResetContent = New(205, nameof(ResetContent));
        public static readonly StatusCode PartialContent = New(206, nameof(PartialContent));
        public static readonly StatusCode MultiStatusWebdav = New(207, nameof(MultiStatusWebdav));
        public static readonly StatusCode AlreadyReportedWebDav = New(208, nameof(AlreadyReportedWebDav));
        public static readonly StatusCode ImUsedHttpDeltaEncoding = New(226, nameof(ImUsedHttpDeltaEncoding));

        // Redirection status codes
        public static readonly StatusCode Redirection = New(300, nameof(Redirection));       // Do not change order
        public static readonly StatusCode MultipleChoice = New(300, nameof(MultipleChoice)); // between these two lines
        public static readonly StatusCode MovedPermanently = New(301, nameof(MovedPermanently));
        public static readonly StatusCode Found = New(302, nameof(Found));
        public static readonly StatusCode SeeOther = New(303, nameof(SeeOther));
        public static readonly StatusCode NotModified = New(304, nameof(NotModified));
        public static readonly StatusCode UseProxy = New(305, nameof(UseProxy));
        public static readonly StatusCode Unused = New(306, nameof(Unused));
        public static readonly StatusCode TemporaryRedirect = New(307, nameof(TemporaryRedirect));
        public static readonly StatusCode PermanentRedirect = New(308, nameof(PermanentRedirect));

        // Client error status codes
        public static readonly StatusCode ClientErrorMin = New(400, nameof(ClientErrorMin)); // Do not change order
        public static readonly StatusCode BadRequest = New(400, nameof(BadRequest));         // between these two lines
        public static readonly StatusCode Unauthorized = New(401, nameof(Unauthorized));
        public static readonly StatusCode PaymentRequired = New(402, nameof(PaymentRequired));
        public static readonly StatusCode Forbidden = New(403, nameof(Forbidden));
        public static readonly StatusCode NotFound = New(404, nameof(NotFound));
        public static readonly StatusCode MethodNotAllowed = New(405, nameof(MethodNotAllowed));
        public static readonly StatusCode NotAcceptable = New(406, nameof(NotAcceptable));
        public static readonly StatusCode ProxyAuthenticationRequired = New(407, nameof(ProxyAuthenticationRequired));
        public static readonly StatusCode RequestTimeout = New(408, nameof(RequestTimeout));
        public static readonly StatusCode Conflict = New(409, nameof(Conflict));
        public static readonly StatusCode Gone = New(410, nameof(Gone));
        public static readonly StatusCode LengthRequired = New(411, nameof(LengthRequired));
        public static readonly StatusCode PreconditionFailed = New(412, nameof(PreconditionFailed));
        public static readonly StatusCode PayloadTooLarge = New(413, nameof(PayloadTooLarge));
        public static readonly StatusCode RequestURITooLong = New(414, nameof(RequestURITooLong));
        public static readonly StatusCode UnsupportedMediaType = New(415, nameof(UnsupportedMediaType));
        public static readonly StatusCode RequestedRangeNotSatisfiable = New(416, nameof(RequestedRangeNotSatisfiable));
        public static readonly StatusCode ExpectationFailed = New(417, nameof(ExpectationFailed));
        public static readonly StatusCode IMATeapot = New(418, nameof(IMATeapot));
        public static readonly StatusCode MisdirectedRequest = New(421, nameof(MisdirectedRequest));
        public static readonly StatusCode UnprocessableEntity = New(422, nameof(UnprocessableEntity));
        public static readonly StatusCode Locked = New(423, nameof(Locked));
        public static readonly StatusCode FailedDependency = New(424, nameof(FailedDependency));
        public static readonly StatusCode UpgradeRequired = New(426, nameof(UpgradeRequired));
        public static readonly StatusCode PreconditionRequired = New(428, nameof(PreconditionRequired));
        public static readonly StatusCode TooManyRequests = New(429, nameof(TooManyRequests));
        public static readonly StatusCode RequestHeaderFieldsTooLarge = New(431, nameof(RequestHeaderFieldsTooLarge));
        public static readonly StatusCode ConnectionClosedWithoutResponse = New(444, nameof(ConnectionClosedWithoutResponse));
        public static readonly StatusCode UnavailableForLegalReasons = New(451, nameof(UnavailableForLegalReasons));
        public static readonly StatusCode ClientErrorMax = New(499, nameof(ClientErrorMax));           // Do not change order
        public static readonly StatusCode ClientClosedRequest = New(499, nameof(ClientClosedRequest)); // between these two lines

        // Server error status codes
        public static readonly StatusCode ServerErrorMin = New(500, nameof(ServerErrorMin));           // Do not change order
        public static readonly StatusCode InternalServerError = New(500, nameof(InternalServerError)); // between these two lines
        public static readonly StatusCode NotImplemented = New(501, nameof(NotImplemented));
        public static readonly StatusCode BadGateway = New(502, nameof(BadGateway));
        public static readonly StatusCode ServiceUnavailable = New(503, nameof(ServiceUnavailable));
        public static readonly StatusCode GatewayTimeout = New(504, nameof(GatewayTimeout));
        public static readonly StatusCode HTTPVersionNotSupported = New(505, nameof(HTTPVersionNotSupported));
        public static readonly StatusCode VariantAlsoNegotiates = New(506, nameof(VariantAlsoNegotiates));
        public static readonly StatusCode InsufficientStorage = New(507, nameof(InsufficientStorage));
        public static readonly StatusCode LoopDetected = New(508, nameof(LoopDetected));
        public static readonly StatusCode NotExtended = New(510, nameof(NotExtended));
        public static readonly StatusCode NetworkAuthenticationRequired = New(511, nameof(NetworkAuthenticationRequired));
        public static readonly StatusCode ServerErrorMax = New(599, nameof(ServerErrorMax));                         // Do not change order
        public static readonly StatusCode NetworkConnectTimeoutError = New(599, nameof(NetworkConnectTimeoutError)); // between these two lines

        static StatusCode New(int statusCode, string statusName)
        {
            var sc = new StatusCode(statusCode, statusName);
            statusCodes[statusCode] = sc;
            return sc;
        }

        public static StatusCode Get(HttpStatusCode statusCode) =>
            Get((int)statusCode);

        public static StatusCode Get(int statusCode) =>
            statusCodes.TryGetValue(statusCode, out var sc) ? sc : NotAKnownStatusCode;

        internal static bool IsClientError(this StatusCode statusCode) => statusCode >= ClientErrorMin && statusCode <= ClientErrorMax;
        internal static bool IsServerError(this StatusCode statusCode) => statusCode >= ServerErrorMin && statusCode <= ServerErrorMax;
        internal static bool IsError(this StatusCode statusCode) => statusCode.IsClientError() || statusCode.IsServerError();
    }
}
