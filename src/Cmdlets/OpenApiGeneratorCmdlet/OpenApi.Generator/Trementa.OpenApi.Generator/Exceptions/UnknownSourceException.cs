using System.Runtime.Serialization;

namespace Trementa.OpenApi.Generator.Exceptions;
[Serializable]
internal class UnknownSourceException : Exception
{
    private object unknownSource;

    public UnknownSourceException()
    {
    }

    public UnknownSourceException(object unknownSource) => this.unknownSource = unknownSource;

    public UnknownSourceException(string? message) : base(message)
    {
    }

    public UnknownSourceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnknownSourceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}