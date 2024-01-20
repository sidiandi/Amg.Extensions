namespace Amg.GetOpt;

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
}
