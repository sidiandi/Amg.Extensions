using Amg.Extensions;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Amg.GetOpt;

public static class Help
{
    public static string? Name => Assembly.GetEntryAssembly()?.GetName()?.Name;

    public static void PrintHelpMessage(TextWriter outputWriter, ICommandProvider commandProvider)
    {
        var c = commandProvider.Commands;
        var o = commandProvider.Options.OrderBy(_ => _.Long);

        var name = Name;

        var w = Wrap(outputWriter);

        var defaultCommand = commandProvider.DefaultCommand();

        var optionsString = o.Any() ? " [options]" : String.Empty;

        if (defaultCommand != null)
        {
            w.WriteLine($"usage: {name}{optionsString} {defaultCommand.ParameterSyntax}");
            w.WriteLine(defaultCommand.Description);
            c = c.Except(new[] { defaultCommand }).OrderBy(_ => _.Name);
        }

        if (c.Count() > 1)
        {
            w.WriteLine();
            w.WriteLine($"usage: {name}{optionsString} <command> [<args>]");
            w.WriteLine("Run a command.");
            w.WriteLine();
            w.WriteLine("Commands:");
            Format(c.Select(_ => new { _.Syntax, _.Description })).Write(w);
        }

        if (o.Any())
        {
            w.WriteLine();
            w.WriteLine("Options:");
            Format(o.Select(_ => new { _.Syntax, _.Description })).Write(w);
        }
    }

    static IWritable Format<T>(IEnumerable<T> e) => Line(e);

    static TextWriter Wrap(TextWriter w, int indent = 2, int pageWidth = 80)
    {
        var wrap = new ActionTextWriter(line =>
        {
            var words = Regex.Split(line, @"\s+");
            int pos = 0;
            bool first = true;
            foreach (var word in words)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (pos == 0)
                    {
                        w.Write(new string(' ', indent));
                        pos = indent;
                    }
                    else
                    {
                        w.Write(' ');
                    }
                }

                w.Write(word);
                pos += word.Length + 1;
                if (pos > pageWidth)
                {
                    w.WriteLine();
                    pos = 0;
                }
            }
            if (pos != 0)
            {
                w.WriteLine();
            }
        });

        return wrap;
    }

    static IWritable Line<T>(IEnumerable<T> e) => TextFormatExtensions.GetWritable(w =>
    {
        foreach (var i in e)
        {
            var p = GetPropertyValues(i).ToArray();
            w.Write(p[0]);
            if (p[1] != null)
            {
                w.Write(" : ");
                w.Write(p[1]);
            }
            w.WriteLine();
        }
    });

    static object?[] GetPropertyValues(object? x)
    {
        if (x == null)
        {
            return new object?[] { };
        }
        var type = x.GetType();
        var properties = type.GetProperties();
        return properties.Select(_ => _.GetValue(x)).ToArray();
    }
}
