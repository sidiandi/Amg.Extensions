using System.Collections.Generic;

namespace Amg.GetOpt;

public interface ICommandProvider
{
    IEnumerable<IOption> Options { get; }
    IEnumerable<ICommand> Commands { get; }

    int? OnOptionsParsed(Parser parser);
}
