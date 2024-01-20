using Amg.Extensions;
using Amg.FileSystem;
using Serilog;
using System.Reflection;

namespace Amg;

public static class Logging
{
    public static string Configure(string? logDir = null)
    {
        if (logDir == null)
        {
            logDir = Assembly.GetExecutingAssembly().Location.Parent().Combine("logs");
        }

        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null) assembly = Assembly.GetExecutingAssembly();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logDir.EnsureDirectoryExists().Combine(
                new[]
                {
                    assembly.GetName().Name,
                    DateTime.UtcNow.ToFileName(),
                    "txt"
                }.Join(".")))
            .CreateLogger();

        return logDir;
    }
}
