using System;

namespace Amg.GetOpt;

public interface IValueParser
{
    /// <summary>
    /// Parses toType from args or throws CommandLineException
    /// </summary>
    /// If an exception is thrown, no items of args are consumed.
    /// <param name="args"></param>
    /// <param name="toType"></param>
    /// <returns>parsed value</returns>
    public object? Parse(ParserState args, Type toType);
}
