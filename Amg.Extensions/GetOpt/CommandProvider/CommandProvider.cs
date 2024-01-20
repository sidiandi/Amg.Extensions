using Amg.Extensions;
using Serilog;
using System.ComponentModel;
using System.Reflection;

namespace Amg.GetOpt
{
    public static class CommandProviderFactory
    {
        public static ICommandProvider FromObject(object objectWithAttributes)
        {
            return new CommandProviderImplementation(objectWithAttributes);
        }

        public static ICommandProvider Log(this ICommandProvider next, ILogger logger)
        {
            return new LoggingCommandProvider(next, logger);
        }
    }

    internal class CommandProviderImplementation : ICommandProvider
    {
        private readonly object commandObject;

        public CommandProviderImplementation(object commandObject)
        {
            this.commandObject = commandObject;
        }

        IEnumerable<ICommandProvider>? commandProviders = null;

        IEnumerable<ICommandProvider> CommandProviders
        {
            get
            {
                if (commandProviders == null)
                {
                    commandProviders = commandObject.GetType().GetProperties()
                        .Where(IsCommandProvider)
                        .Select(_ => _.GetValue(commandObject))
                        .NotNull()
                        .Select(_ => new CommandProviderImplementation(_))
                        .ToList();
                }
                return commandProviders;
            }
        }

        static bool IsCommandProvider(PropertyInfo p)
            => p.GetCustomAttribute<CommandProviderAttribute>() != null;

        IEnumerable<ICommand>? commands = null;
        public IEnumerable<ICommand> Commands
        {
            get
            {
                if (commands == null)
                {
                    var type = commandObject.GetType();
                    commands = type.GetMethods()
                        .Where(IsCommand)
                        .Where(_ => _.DeclaringType == type)
                        .Select(_ => new Command(commandObject, _))
                        .Concat(CommandProviders.SelectMany(_ => _.Commands))
                        .DistinctBy(_ => _.Name);
                }
                return commands;
            }
        }

        public static bool IsCommand(MethodInfo m)
        {
            return HasDescriptionAttribute(m) ||
                (!HasDescriptionAttributes(m.DeclaringType!) && m.IsPublic && !m.IsSpecialName);
        }

        static bool IsOption(PropertyInfo p)
        {
            return HasDescriptionAttribute(p) ||
                !HasDescriptionAttributes(p.DeclaringType!) && p.SetMethod is { } && p.SetMethod.IsPublic;
        }

        internal static bool HasDescriptionAttribute(PropertyInfo p)
            => p.GetCustomAttribute<DescriptionAttribute>() != null;

        internal static bool HasDescriptionAttribute(MethodInfo m)
            => m.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>() != null;

        internal static bool HasDescriptionAttributes(Type type)
        {
            return Array.Exists(type.GetMethods(), _ => HasDescriptionAttribute(_)) ||
                Array.Exists(type.GetProperties(), _ => HasDescriptionAttribute(_));
        }

        IEnumerable<IOption>? options = null;
        public IEnumerable<IOption> Options
        {
            get
            {
                if (options == null)
                {
                    options = commandObject.GetType().GetProperties()
                    .Where(IsOption)
                    .Select(_ => new Option(commandObject, _))
                    .Concat(CommandProviders.SelectMany(_ => _.Options))
                    .DistinctBy(_ => _.Long);
                }
                return options;
            }
        }

        public int? OnOptionsParsed(Parser parser)
        {
            if (commandObject is ICommandObject commandObjectInterface)
            {
                var exitCode = commandObjectInterface.OnOptionsParsed(parser);
                if (exitCode != null)
                {
                    return exitCode;
                }
            }

            return CommandProviders.Select(_ => _.OnOptionsParsed(parser))
                .FirstOrDefault(_ => _ != null);
        }
    }
}

