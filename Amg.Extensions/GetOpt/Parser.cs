using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Amg.GetOpt;

public class Parser
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    private readonly ICommandProvider commandProvider;
    private readonly ValueParser valueParser = new ValueParser();

    ParserState state;
    bool handleArgs = false;

    public ICommandProvider CommandProvider => commandProvider;

    readonly List<ParserState> operands = new List<ParserState>();

    public IEnumerable<string> Operands => operands.Select(_ => _.Current);

    public Parser(ICommandProvider commandProvider)
    {
        this.commandProvider = commandProvider;
        this.state = new ParserState(new string[] { });
    }

    public bool IgnoreUnknown { get; set; } = false;

    public void Parse(string[] args)
    {
        this.state = new ParserState(args);
        handleArgs = true;
        operands.Clear();

        while (state.HasCurrent)
        {
            Logger.Information(@"Parsing command line option:
{state}", state);

            if (OptionStop())
            {
                // nothing
            }
            else if (LongOption())
            {
                // nothing
            }
            else if (ShortOption())
            {
                // nothing
            }
            else
            {
                Operand();
            }
        }
    }

    ICommand? DefaultCommand => commandProvider.Commands.FirstOrDefault(_ => _.IsDefault);

    public async Task<object?[]> Run()
    {
        var results = new List<object?>();
        var args = new ParserState(Operands.ToArray());


        if (!args.HasCurrent)
        {
            var defaultCommand = DefaultCommand;
            if (defaultCommand == null)
            {
                throw new NoDefaultCommandException();
            }
            else
            {
                results.Add(await defaultCommand.Invoke(args, valueParser));
            }
        }
        else
        {
            while (args.HasCurrent)
            {
                var defaultCommand = DefaultCommand;
                if (defaultCommand == null)
                {
                    var commandName = args.Current;
                    var command = commandProvider.GetCommand(commandName);
                    args.Consume();
                    results.Add(await command.Invoke(args, valueParser));
                }
                else
                {
                    // command found?
                    // execute
                    // else
                    // execute default command
                    // check that is consumes arguments to avoid endless loop

                    var commandName = args.Current;
                    var command = commandProvider.TryGetCommand(commandName);
                    if (command is { })
                    {
                        args.Consume();
                        results.Add(await command.Invoke(args, valueParser));
                    }
                    else
                    {
                        var tempArgs = args.Clone();
                        results.Add(await defaultCommand.Invoke(args, valueParser));
                        if (tempArgs.Equals(args))
                        {
                            throw new CommandLineException(args, "unexpected operands");
                        }
                    }
                }

            }
        }
        return results.ToArray();
    }

    private void Operand()
    {
        operands.Add(state.Clone());
        state.Consume();
    }

    private bool ShortOption()
    {
        var p = Current.Split(new[] { shortOptionPrefix }, 2, StringSplitOptions.None);
        if (p.Length > 1 && String.IsNullOrEmpty(p[0]) && handleArgs)
        {
            state.Consume();
            string? optionText = p[1];

            while (optionText != null)
            {
                var keyValue = SplitFirstCharacter(optionText);
                var name = keyValue[0];
                var value = keyValue.Length > 1 ? keyValue[1] : null;

                IOption? option = null;
                try
                {
                    option = commandProvider.GetShortOption(name);
                }
                catch (ArgumentException argumentException)
                {
                    if (IgnoreUnknown)
                    {
                        return false;
                    }
                    else
                    {
                        throw new CommandLineException(state,
                            argumentException.Message,
                            argumentException);
                    }
                }
                option.Set(ref value, state, valueParser);
                optionText = value;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    T Check<T>(Func<T> f) => Check(this.state, f);

    internal static T Check<T>(ParserState args, Func<T> f)
    {
        try
        {
            return f();
        }
        catch (Exception e)
        {
            throw new CommandLineException(args, e.Message);
        }
    }

    static string[] SplitFirstCharacter(string x)
    {
        if (x.Length == 0)
        {
            return new string[] { };
        }
        else if (x.Length == 1)
        {
            return new string[] { x };
        }
        else
        {
            return new string[] { x.Substring(0, 1), x.Substring(1) };
        }
    }

    private bool LongOption()
    {
        var p = Current.Split(new[] { longOptionPrefix }, 2, StringSplitOptions.None);
        if (p.Length > 1 && String.IsNullOrEmpty(p[0]) && handleArgs)
        {
            state.Consume();
            var keyValue = p[1].Split(new[] { "=" }, 2, StringSplitOptions.None);
            var name = keyValue[0];
            var value = keyValue.Length > 1 ? keyValue[1] : null;

            IOption? option = null;
            try
            {
                option = commandProvider.GetLongOption(name);
            }
            catch (ArgumentException argumentException)
            {
                if (IgnoreUnknown)
                {
                    return false;
                }
                else
                {
                    throw new CommandLineException(state,
                        argumentException.Message,
                        argumentException);
                }
            }
            option.Set(ref value, state, valueParser);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool OptionStop()
    {
        if (Current.Equals(longOptionPrefix))
        {
            state.Consume();
            handleArgs = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    string Current => state.Current;

    internal const string longOptionPrefix = "--";
    internal const string shortOptionPrefix = "-";

    public static string LongNameForCsharpIdentifier(string memberName)
    {
        return new string(memberName.Take(1)
            .Select(char.ToLower)
            .Concat(memberName.Skip(1)
                .SelectMany(_ => char.IsUpper(_)
                    ? new[]
                    {
                            '-',
                            char.ToLower(_)
                    }
                    : new[]
                    {
                            _
                    }))
            .ToArray());
    }

    public static string LongName(PropertyInfo p)
        => LongNameForCsharpIdentifier(p.Name);

    public static string LongName(MethodInfo m)
        => LongNameForCsharpIdentifier(m.Name);
}
