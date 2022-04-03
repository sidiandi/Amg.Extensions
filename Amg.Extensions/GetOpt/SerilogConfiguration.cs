using Serilog;
using Serilog.Events;
using System.ComponentModel;

namespace Amg.GetOpt;

public enum Verbosity
{
    Quiet,
    Minimal,
    Normal,
    Detailed
}

public class SerilogConfiguration : ICommandObject
{
    [Short('v'), Description("Logging verbosity")]
    public Verbosity Verbosity { get; set; } = Verbosity.Quiet;

    public Serilog.Core.LoggingLevelSwitch LoggingLevelSwitch { get; }

    static LogEventLevel GetLogEventLevel(Verbosity v)
        => v switch
        {
            Verbosity.Quiet => LogEventLevel.Fatal,
            Verbosity.Minimal => LogEventLevel.Error,
            Verbosity.Normal => LogEventLevel.Information,
            _ => LogEventLevel.Verbose,
        };

    public SerilogConfiguration()
    {
        LoggingLevelSwitch = new Serilog.Core.LoggingLevelSwitch(LogEventLevel.Fatal);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .WriteTo
            .Console(restrictedToMinimumLevel: GetLogEventLevel(Verbosity))
            .CreateLogger();
    }

    public int? OnOptionsParsed(Parser parser)
    {
        LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(Verbosity);
        return null;
    }
}
