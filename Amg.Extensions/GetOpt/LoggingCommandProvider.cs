using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Amg.GetOpt;

class LoggingCommandProvider : ICommandProvider
{
    private readonly ICommandProvider next;
    private readonly ILogger logger;

    public LoggingCommandProvider(ICommandProvider next, ILogger logger)
    {
        this.next = next;
        this.logger = logger;
    }

    class Command : ICommand
    {
        private readonly ICommand next;

        public Command(ICommand next, ILogger logger)
        {
            this.next = next;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public string Name => next.Name;

        public string Syntax => next.Syntax;

        public string? Description => next.Description;

        public bool IsDefault => next.IsDefault;

        public string ParameterSyntax => next.ParameterSyntax;

        public Task<object?> Invoke(ParserState args, IValueParser valueParser)
        {
            Logger.Information("Invoke {command}", Name);
            return next.Invoke(args, valueParser);
        }
    }

    class Option : IOption
    {
        private readonly IOption next;
        private readonly ILogger logger;

        public Option(IOption next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public string Long => next.Long;

        public string? Short => next.Short;

        public string Syntax => next.Syntax;

        public string? Description => next.Description;

        public void Set(ref string? appendedValue, ParserState args, IValueParser valueParser)
        {
            logger.Information("Set {option}", Long);
            next.Set(ref appendedValue, args, valueParser);
        }
    }

    public IEnumerable<IOption> Options => next.Options
        .Select(_ => new Option(_, logger));

    public IEnumerable<ICommand> Commands => next.Commands
        .Select(_ => new Command(_, logger));

    public int? OnOptionsParsed(Parser parser) => next.OnOptionsParsed(parser);
}
