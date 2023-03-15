using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OpenApi.Library.Types.Error;

using Types.Error.Exceptions;
using NotImplementedException = Exceptions.NotImplementedException;
using static StatusCodes;
using OpenApi.Library.Types.Error.Exceptions;

public static class ResponseErrorExceptionFactory
{
    static readonly Dictionary<StatusCode, Type> exceptionMap = new Dictionary<StatusCode, Type>
    {
     { BadRequest                     , typeof(BadRequestException)},
     { Unauthorized                   , typeof(UnauthorizedException)},
     { PaymentRequired                , typeof(PaymentRequiredException)},
     { Forbidden                      , typeof(ForbiddenException)},
     { NotFound                       , typeof(NotFoundException)},
     { MethodNotAllowed               , typeof(MethodNotAllowedException)},
     { NotAcceptable                  , typeof(NotAcceptableException)},
     { ProxyAuthenticationRequired    , typeof(ProxyAuthenticationRequiredException)},
     { RequestTimeout                 , typeof(RequestTimeoutException)},
     { Conflict                       , typeof(ConflictException)},
     { Gone                           , typeof(GoneException)},
     { LengthRequired                 , typeof(LengthRequiredException)},
     { PreconditionFailed             , typeof(PreconditionFailedException)},
     { PayloadTooLarge                , typeof(PayloadTooLargeException)},
     { RequestURITooLong              , typeof(RequestURITooLongException)},
     { UnsupportedMediaType           , typeof(UnsupportedMediaTypeException)},
     { RequestedRangeNotSatisfiable   , typeof(RequestedRangeNotSatisfiableException)},
     { ExpectationFailed              , typeof(ExpectationFailedException)},
     { IMATeapot                      , typeof(IMATeapotException)},
     { MisdirectedRequest             , typeof(MisdirectedRequestException)},
     { UnprocessableEntity            , typeof(UnprocessableEntityException)},
     { Locked                         , typeof(LockedException)},
     { FailedDependency               , typeof(FailedDependencyException)},
     { UpgradeRequired                , typeof(UpgradeRequiredException)},
     { PreconditionRequired           , typeof(PreconditionRequiredException)},
     { TooManyRequests                , typeof(TooManyRequestsException)},
     { RequestHeaderFieldsTooLarge    , typeof(RequestHeaderFieldsTooLargeException)},
     { ConnectionClosedWithoutResponse, typeof(ConnectionClosedWithoutResponseException)},
     { UnavailableForLegalReasons     , typeof(UnavailableForLegalReasonsException)},
     { ClientClosedRequest            , typeof(ClientClosedRequestException)},
     { InternalServerError            , typeof(InternalServerErrorException)},
     { NotImplemented                 , typeof(NotImplementedException)},
     { BadGateway                     , typeof(BadGatewayException)},
     { ServiceUnavailable             , typeof(ServiceUnavailableException)},
     { GatewayTimeout                 , typeof(GatewayTimeoutException)},
     { HTTPVersionNotSupported        , typeof(HTTPVersionNotSupportedException)},
     { VariantAlsoNegotiates          , typeof(VariantAlsoNegotiatesException)},
     { InsufficientStorage            , typeof(InsufficientStorageException)},
     { LoopDetected                   , typeof(LoopDetectedException)},
     { NotExtended                    , typeof(NotExtendedException)},
     { NetworkAuthenticationRequired  , typeof(NetworkAuthenticationRequiredException)},
     { NetworkConnectTimeoutError     , typeof(NetworkConnectTimeoutErrorException)},
    };

    public static ApiException Exception(HttpResponseMessage response, object content, Exception innerException = null) =>
        CreateException(Get(response.StatusCode), content, null, innerException) as ApiException;


    public static ApiException Exception(StatusCode statusCode, object content = null, string message = null, Exception innerException = null) =>
        CreateException(statusCode, content, message, innerException) as ApiException;

    static object CreateException(StatusCode statusCode, object content = null, string message = null, Exception innerException = null)
    {
        if (exceptionMap.TryGetValue(statusCode, out var type))
            return Activator.CreateInstance(type, content, message, innerException);
        if (statusCode.IsClientError())
            return Activator.CreateInstance(typeof(ClientErrorException), statusCode, content, message, innerException);
        if (statusCode.IsServerError())
            return Activator.CreateInstance(typeof(ServerErrorException), statusCode, content, message, innerException);
        return new ResponseErrorException(statusCode, content, message, innerException);
    }
}
