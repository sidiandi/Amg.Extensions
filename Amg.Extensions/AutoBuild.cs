using Amg.FileSystem;
using System;
using System.IO;
using System.Reflection;

namespace Amg;

public static class AutoBuild
{
    /// <summary>
    /// Wrapper for a Main method that checks if the executable is out-of-date.
    /// </summary>
    /// Exits with exit code 9009 if the source files for the executable exist and 
    /// are newer than the executable
    /// Should be combined with a bootstrapper .cmd script like this:
    /// 
    /// @echo off
    /// set name =% ~n0
    /// set conf = Debug
    /// set fw = net6.0
    /// set exe =% ~dp0 % name %\bin\%conf%\%fw%\%name%.exe
    /// %exe% %*
    /// IF %ERRORLEVEL% EQU 9009 (
    ///     dotnet build %~dp0%name%
    ///     dotnet test %~dp0%name%
    ///     %exe% %*
    /// )
    /// 
    /// <param name="args"></param>
    /// <param name="main"></param>
    /// <returns></returns>
    public static int Main(string[]? args = null, Func<string[],int>? main = null)
    {
        if (args == null)
        {
            args = System.Environment.GetCommandLineArgs().Skip(1).ToArray();
        }
        if (main == null)
        {
            main = _ => Amg.GetOpt.GetOpt.Main(_);
        }

        var exe = Assembly.GetEntryAssembly()!.Location;
        var sourceDir = SourceDir(exe);
        if (sourceDir is { })
        {
            var sourceFiles = new Glob(sourceDir)
                .Include("*.cs")
                .Include("*.csproj")
                .EnumerateFileInfos();
            var lastChange = sourceFiles.Select(_ => _.LastWriteTimeUtc).Max();
            var needsUpdate = new FileInfo(exe).LastWriteTimeUtc < lastChange;
            if (needsUpdate)
            {
                Console.Error.WriteLine("Source files have changed. Build required.");
                System.Environment.Exit(9009); // this exit code is returned in cmd if the executable does not exist.
            }
        }
        return main(args);
    }

    /// <summary>
    /// Get source dir
    /// </summary>
    /// Get source dir from an executable generated with `dotnet build`
    /// Example: C:\src\Amg.Extensions\examples\GetOpt\bin\Debug\net6.0\GetOpt.exe => C:\src\Amg.Extensions\examples\GetOpt
    /// <param name="exe">Executable path</param>
    /// <returns>Source dir or null if not found.</returns>
    static string? SourceDir(string exe)
    {
        exe = exe.Absolute();
        var name = exe.FileNameWithoutExtension();
        var binDir = exe.ParentOrNull()?.ParentOrNull()?.ParentOrNull();
        if (binDir is null || !binDir.FileName().Equals("bin"))
        {
            return null;
        }
        var sourceDir = binDir.ParentOrNull();
        if (sourceDir is null || !sourceDir.Combine(name).CatName(".csproj").IsFile())
        {
            return null;
        }
        return sourceDir;
    }
}
