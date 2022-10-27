using Amg.FileSystem;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Amg.Test;

public static class TestHelpers
{
    public static Serilog.ILogger Logger([CallerFilePath] string? file = null)
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        return Serilog.Log.Logger.ForContext("File", file);
    }

    public static string GetThisFilePath([CallerFilePath] string? path = null) => path!;

    /// <summary>
    /// Create an empty directory to read and write to during a single test. Previous test output will be deleted.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreateTestDirectory([CallerMemberName] string? name = null)
    {
        return Assembly.GetExecutingAssembly()
            .Directory()
            .Combine("test", name!).EnsureDirectoryIsEmpty();
    }

    /// <summary>
    /// Create a directory to read and write to during a single test. Previous test output will be kept.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreatePersistentTestDirectory([CallerMemberName] string? name = null)
    {
        return Assembly.GetExecutingAssembly()
            .Directory()
            .Combine("test.p", name!).EnsureDirectoryExists();
    }

    public static void Dump<T>(IEnumerable<T> e)
    {
        foreach (var i in e)
        {
            Console.WriteLine(i);
        }
    }

    /// <summary>
    /// Access static test data which is stored in the "test-data" directory in the source tree.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
    public static string GetTestDataDirectory()
    {
        var d = Assembly.GetCallingAssembly().Location.Parent().Combine("test-data");
        if (!d.IsDirectory())
        {
            throw new System.IO.DirectoryNotFoundException(d);
        }
        return d;
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
