using Amg.Extensions;
using System.Linq;

namespace Amg.GetOpt;

internal static class CommandProviderExtensions
{
    public static ICommand GetCommand(this ICommandProvider commandProvider, string name)
    {
        return commandProvider.Commands.FindByName(_ => _.Name, name, "commands");
    }

    public static ICommand? TryGetCommand(this ICommandProvider commandProvider, string name)
    {
        return commandProvider.Commands.FindByNameOrDefault(_ => _.Name, name);
    }

    public static IOption GetLongOption(this ICommandProvider commandProvider, string optionName)
    {
        return commandProvider.Options.FindByName(_ => _.Long, optionName, "options");
    }

    public static IOption GetShortOption(this ICommandProvider commandProvider, string optionName)
    {
        return commandProvider.Options
            .Where(_ => _.Short != null)
            .FindByName(_ => _.Short, optionName, "options");
    }

    public static ICommand? DefaultCommand(this ICommandProvider commandProvider)
    {
        var c = commandProvider.Commands.ToList();
        if (c.Count == 0)
        {
            return null;
        }
        else if (c.Count == 1)
        {
            return c[0];
        }
        else
        {
            return c.FirstOrDefault(_ => _.IsDefault);
        }
    }
}
