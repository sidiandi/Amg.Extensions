using Amg.Extensions;
using Amg.FileSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amg.FileSystem;

public static class ChildProcess
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

    /// <summary>
    /// Quote only if x contains whitespace.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static string QuoteIfRequired(string x)
    {
        if (string.IsNullOrEmpty(x)) return "\"\"";
        return x.Any(char.IsWhiteSpace)
            ? x.Quote()
            : x;
    }

    /// <summary>
    /// Quotes a string.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="quotes">leading quote, trailing quote and escape character. Example: "[]\\" </param>
    /// <returns></returns>
    public static string Quote(string x, string leadingQuote, string trailingQuote, string? escape = null)
    {
        if (escape is { })
        {
            x = Regex.Replace(x, $"{Regex.Escape(leadingQuote)}|{Regex.Escape(trailingQuote)}",
                new MatchEvaluator(m => escape + m.Value));
        }
        return leadingQuote + x + trailingQuote;
    }

    /// <summary>
    /// Quotes a string.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static string Quote(string x) => TextFormatExtensions.Quote(x, "\"", "\"", "\\");

    /// <summary>
    /// Creates an argument string for Process.StartInfo.Arguments
    /// </summary>
    /// Provided as a convenience for use outside of this class.
    /// Quotes arguments with whitespace
    /// <param name="args"></param>
    /// <returns></returns>
    public static string CreateArgumentsString(IEnumerable<object> args)
    {
        return string.Join(" ", args.Select(_ => DecodeArg(_).QuoteIfRequired()));
    }

    public static string Decode(SecureString s)
    {
        var n = new NetworkCredential("dummy", s);
        return n.Password;
    }

    public static string DecodeArg(object x)
    {
        if (x is SecureString s)
        {
            return Decode(s);
        }
        else
        {
            return x is null 
                ? String.Empty
                : x.ToString() ?? String.Empty;
        }
    }

    public static SecureString ToSecureString(string x)
    {
        var s = new SecureString();
        foreach (var i in x)
        {
            s.AppendChar(i);
        }
        return s;
    }

    /// <summary>
    /// Quote a string with ' ' to be used in a powershell command
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static string PsQuote(string x)
    {
        const string quote = "'";
        return TextFormatExtensions.Quote(x, quote, quote, quote);
    }

    public static async Task<Result> Run(
        string fileName,
        IEnumerable<object> args,
        IDictionary<string, string>? environment = null)
    {
        var process = new Process();

        var startInfo = process.StartInfo;
        startInfo.FileName = fileName;
        startInfo.Arguments = CreateArgumentsString(args);
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        if (environment != null)
        {
            foreach (var i in environment)
            {
                startInfo.EnvironmentVariables.Add(i.Key, i.Value);
            }
        }

        process.Start();

        Logger.Information($@"Process {process.Id} started:
{startInfo.FileName} `
{string.Join("\r\n", args.Select(_ => "  " + _.ToString()!.QuoteIfRequired() + " `"))}");

        var output = ReadAndPrint($"{process.Id}:1>", process.StandardOutput);
        var error = ReadAndPrint($"{process.Id}:2>", process.StandardError);

        await Task.WhenAll(output, error, process.WaitForExitAsync());

        Logger.Information("Process {id} ended with exit code {exitCode}.", process.Id, process.ExitCode);

        return new Result(
            await output,
            await error,
            process.ExitCode);
    }

    static async Task<string> ReadAndPrint(string prefix, TextReader r)
    {
        var s = new StringWriter();
        while (true)
        {
            var line = await r.ReadLineAsync();
            if (line == null) return s.ToString();
            Logger.Information(prefix + line);
            s.WriteLine(line);
        }
    }

    public record Result
    {
        public Result(string output, string error, int exitCode)
        {
            Output = output;
            Error = error;
            ExitCode = exitCode;
        }

        public string Output { get; private set; }
        public string Error { get; private set; }
        public int ExitCode { get; private set; }

    }

}
