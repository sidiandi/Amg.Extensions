using Amg.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Amg.GetOpt.Test")]

namespace Amg.GetOpt;

public static class ExitCode
{
    public const int Success = 0;
    public const int UnknownError = 1;
    public const int HelpDisplayed = 3;
    public const int CommandLineError = 4;
    public const int CommandFailed = 5;
}

class WithStandardOptions
{
    public WithStandardOptions(object commandObject)
    {
        this.CommandObject = commandObject;
    }

    [CommandProvider]
    public StandardOptions StandardOptions { get; } = new StandardOptions();
    [CommandProvider]
    public object CommandObject { get; }
}


public static class GetOpt
{
    public static int Main(string[] args)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var classWithMainMethod = entryAssembly?.EntryPoint?.DeclaringType;
        if (classWithMainMethod is null)
        {
            throw new InvalidOperationException();
        }
        var program = Activator.CreateInstance(classWithMainMethod);
        if (program is null)
        {
            throw new InvalidOperationException();
        }
        return Main(args, program);
    }

    public static int Main(string[] args, object commandObject)
    {
        var commandProvider = CommandProviderFactory.FromObject(new WithStandardOptions(commandObject));
        return Run(args, commandProvider);
    }

    public static int Run(string[] args, ICommandProvider commandProvider)
    {
        var parser = new Parser(commandProvider);

        parser.Parse(args);

        var onOptionsParsedExitCode = commandProvider.OnOptionsParsed(parser);
        if (onOptionsParsedExitCode != null)
        {
            return onOptionsParsedExitCode.Value;
        }

        try
        {
            var result = parser.Run().Result;
            foreach (var i in result.NotNull())
            {
                Console.WriteLine(i);
            }
        }
        catch (AggregateException aex)
        {
            int exitCode = ExitCode.Success;

            aex.Handle(exception =>
            {
                if (exception is NoDefaultCommandException)
                {
                    Help.PrintHelpMessage(Console.Out, commandProvider);
                    exitCode = ExitCode.HelpDisplayed;
                    return true;
                }
                if (exception is CommandLineException cex)
                {
                    Console.Error.WriteLine($@"{cex.ErrorMessage}

See {Help.Name} --help"
);
                    exitCode = ExitCode.CommandLineError;
                    return true;
                }
                else
                {
                    Console.Error.WriteLine(exception);
                    exitCode = ExitCode.UnknownError;
                    return true;
                }
            });

            return exitCode;
        }

        return ExitCode.Success;
    }
}
