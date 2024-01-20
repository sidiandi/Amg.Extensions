namespace Amg.GetOpt;

public class CommandLineException : Exception
{
    public ParserState? Args { get; }

    public CommandLineException()
    {
    }

    public CommandLineException(ParserState args, string message)
        : base(message)
    {
        this.Args = args;
    }

    public CommandLineException(ParserState args, string message, Exception innerException)
        : base(message, innerException)
    {
        this.Args = args;
    }

    public CommandLineException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public string ErrorMessage => $@"{Message}

{Args}
";
}
