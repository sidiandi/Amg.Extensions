namespace Amg.OnceImpl;

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
}

