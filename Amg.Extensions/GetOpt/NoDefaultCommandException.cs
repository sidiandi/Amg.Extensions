using System;
using System.Runtime.Serialization;

namespace Amg.GetOpt;

[Serializable]
public class NoDefaultCommandException : Exception
{
    public NoDefaultCommandException()
    {
    }

    public NoDefaultCommandException(string message) : base(message)
    {
    }

    public NoDefaultCommandException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NoDefaultCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
