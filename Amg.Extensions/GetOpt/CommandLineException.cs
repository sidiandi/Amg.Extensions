using System;
using System.Runtime.Serialization;

namespace Amg.GetOpt;

[Serializable]
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

    protected CommandLineException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public string ErrorMessage => $@"{Message}

{Args}
";
}
