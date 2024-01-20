using Amg.Extensions;
using Serilog;
using System.Runtime.CompilerServices;

namespace Amg;

public static class SerilogExtensions
{
    public static void Dump(this ILogger logger, object x, [CallerFilePath] string? sourceFile = null, [CallerLineNumber] int sourceLine = 0)
    {
        try
        {
            var properties = x.GetType().GetProperties();
            var messageTemplate = $@"{{sourceFile}}({{sourceLine}}):
" + properties.Select(_ => $"  {_.Name} = {{{_.Name}}}").Join();
            var propertyValues = new object[] { sourceFile!, sourceLine }.Concat(properties.Select(_ => _.GetValue(x))).ToArray();
            logger.Information(messageTemplate, propertyValues);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Cannot evaluate {object}", x);
        }
    }
}
