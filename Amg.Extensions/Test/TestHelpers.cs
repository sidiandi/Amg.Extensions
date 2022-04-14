using Amg.FileSystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Amg.Test;

public static class TestHelpers
{
    public static string GetThisFilePath([CallerFilePath] string? path = null) => path!;

    public static string CreateTestDirectory([CallerMemberName] string? name = null)
    {
        return GetThisFilePath().Parent().Combine("out", "test", name!).EnsureDirectoryIsEmpty();
    }

    public static TimeSpan MeasureTime(Action a)
    {
        var stopwatch = Stopwatch.StartNew();
        a();
        return stopwatch.Elapsed;
    }

    public static async Task<TimeSpan> MeasureTime(Func<Task> a)
    {
        var stopwatch = Stopwatch.StartNew();
        await a();
        return stopwatch.Elapsed;
    }

    public static (string output, string error) CaptureOutput(Action action)
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
