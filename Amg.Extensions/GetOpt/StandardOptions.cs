using System;
using System.ComponentModel;
using System.Reflection;

namespace Amg.GetOpt;

public class StandardOptions : ICommandObject
{
    [Short('h'), Description("Print help and exit.")]
    public bool Help { get; set; }

    [Description("Print version and exit.")]
    public bool Version { get; set; }

    [CommandProvider]
    public SerilogConfiguration SerilogConfiguration { get; } = new SerilogConfiguration();

    public int? OnOptionsParsed(Parser parser)
    {
        if (Help)
        {
            Amg.GetOpt.Help.PrintHelpMessage(Console.Out, parser.CommandProvider);
            return ExitCode.HelpDisplayed;
        }

        if (Version)
        {
            var version = Assembly.GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            if (version is { })
            {
                Console.WriteLine(version);
            }
        }

        return null;
    }
}
