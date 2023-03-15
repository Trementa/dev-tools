#nullable disable 
using System.Runtime.Serialization;

namespace OpenApi.Library.Types.Exceptions;

[Serializable]
public class UnknownTypeException : Exception
{
    public readonly Type ServiceType;
    public UnknownTypeException(Type serviceType) => ServiceType = serviceType;

    protected UnknownTypeException()
    { }


    protected UnknownTypeException(string message) : base(message)
    { }

    protected UnknownTypeException(string message, Exception innerException) : base(message, innerException)
    { }

    protected UnknownTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
    { }
}