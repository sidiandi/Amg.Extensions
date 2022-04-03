using System;
using System.Runtime.Serialization;

namespace Amg.OnceImpl;

[Serializable]
public class OnceException : Exception
{
    public OnceException()
    {
    }

    public OnceException(string message) : base(message)
    {
    }

    public OnceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected OnceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

