
namespace Amg.GetOpt;

public interface IOption
{
    string Long { get; }
    string? Short { get; }
    void Set(ref string? appendedValue, ParserState args, IValueParser valueParser);
    string Syntax { get; }
    string? Description { get; }
}
