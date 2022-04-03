using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Amg.GetOpt.Test;

public class TestBase
{
    protected TestBase()
    {

    }

    static TestBase()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();
    }

    public static TimeSpan MeasureTime(Action a)
    {
        var stopwatch = Stopwatch.StartNew();
        a();
        return stopwatch.Elapsed;
    }

    public static TimeSpan MeasureTime(Func<Task> a)
    {
        var stopwatch = Stopwatch.StartNew();
        a().Wait();
        return stopwatch.Elapsed;
    }

    public (string output, string error) CaptureOutput(Action action)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var captureOut = new StringWriter();
        var captureError = new StringWriter();
        try
        {
            Console.SetOut(captureOut);
            Console.SetError(captureError);
            action();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        return (captureOut.ToString(), captureError.ToString());
    }
}
