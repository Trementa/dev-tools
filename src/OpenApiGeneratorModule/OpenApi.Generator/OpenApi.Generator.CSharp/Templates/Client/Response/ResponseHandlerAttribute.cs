using System;

namespace Templates.Client.Response;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ResponseHandlerAttribute : Attribute
{
    public Type ResponseHandlerType { get; }
    public ResponseHandlerAttribute(Type responseHandlerType)
    {
        if (responseHandlerType is not ICustomResponseHandler)
            throw new InvalidOperationException(responseHandlerType.ToString());
        ResponseHandlerType = responseHandlerType;
    }

    public ICustomResponseHandler GetResponseHandler() =>
        throw new NotImplementedException();
}
