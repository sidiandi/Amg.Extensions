using System.IO;
using System.Reflection;

namespace Amg.Extensions;

/// <summary>
/// Mixed extensions
/// </summary>
public static class Utils
{
    public static string HumanReadable(this TimeSpan? duration)
    {
        return duration is { }
            ? duration.Value.HumanReadable()
            : String.Empty;
    }

    /// <summary>
    /// Easy readable text format for a TimeSpan
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static string HumanReadable(this TimeSpan duration)
    {
        var days = duration.TotalDays;
        if (days > 10)
        {
            return $"{days:F0}d";
        }
        if (days > 1)
        {
            return $"{days:F0}d{duration.Hours}h";
        }
        var hours = duration.TotalHours;
        if (hours > 1)
        {
            return $"{duration.Hours}h{duration.Minutes}m";
        }
        var minutes = duration.TotalMinutes;
        if (minutes > 30)
        {
            return $"{duration.Minutes}m";
        }
        if (minutes > 1)
        {
            return $"{duration.Minutes}m{duration.Seconds}s";
        }
        return $"{duration.Seconds}s";
    }

    /// <summary>
    /// Transforms input into a TextReader that reads from input and outputs all characters read on TextWriter output as well.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static TextReader Tee(this TextReader input, TextWriter output)
    {
        return new TeeTextReader(input, output);
    }


    /// <summary>
    /// Transforms input into a TextReader that reads from input and calls outputs on all lines read.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static TextReader Tee(this TextReader input, Action<string> output)
    {
        return new TeeTextReader(input, output.AsTextWriter());
    }

    /// <summary>
    /// Returns a TextWriter that calls output for every WriteLine
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public static TextWriter AsTextWriter(this Action<string> output)
    {
        return new ActionTextWriter(output);
    }

    /// <summary>
    /// Returns a TextWriter that indents
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public static TextWriter Indent(this TextWriter dest, string prefix)
    {
        return new ActionTextWriter(_ =>
        {
            dest.Write(prefix);
            dest.WriteLine(_);
        });
    }

    /// <summary>
    /// Limit x in [a,b]
    /// </summary>
    public static DateTime Limit(this DateTime x, DateTime a, DateTime b)
    {
        if (x < a)
        {
            return a;
        }
        else
        {
            if (x > b)
            {
                return b;
            }
            else
            {
                return x;
            }
        }
    }

    /// <summary>
    /// Limit x in [a,b]
    /// </summary>
    public static int Limit(this int x, int a, int b)
    {
        if (x < a)
        {
            return a;
        }
        else
        {
            if (x > b)
            {
                return b;
            }
            else
            {
                return x;
            }
        }
    }

    /// <summary>
    /// Returns the nuget version of the assembly.
    /// </summary>
    /// Expects this information in assembly metadata "NuGetVersionV2"
    /// <param name="a"></param>
    /// <returns></returns>
    public static string NugetVersion(this Assembly a) => a
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(_ => _.Key.Equals("NuGetVersionV2")).Value!;
}
