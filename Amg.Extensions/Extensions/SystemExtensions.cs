using System.Text.RegularExpressions;
using System.Reflection;

namespace Amg.Extensions;

public static class SystemExtensions
{
    public static IEnumerable<string> SourceLocationsFromStackTrace(string? stackTrace)
    {
        if (stackTrace == null) return Enumerable.Empty<string>();
        return Regex.Matches(stackTrace, @"in (.*):line (\d+)").Cast<Match>()
            .Select(m => new { file = m.Groups[1].Value, line = m.Groups[2].Value })
            .Select(_ => $"{_.file}({_.line})");
    }

    static internal string Fullname(this MethodInfo method)
    {
        return method.DeclaringType!.FullName + "." + method.Name;
    }

    public static bool Has<T>(this ICustomAttributeProvider attributeProvider)
    => attributeProvider.IsDefined(typeof(T), true);

    public static string FullName(this MethodInfo m) => $"{m.DeclaringType?.FullName}.{m.Name}";

    internal static IEnumerable<string> SourceLocations(this Exception exception)
    {
        return exception.StackTrace is null
            ? Enumerable.Empty<string>()
            : SourceLocationsFromStackTrace(exception.StackTrace);
    }

    public static bool IsSetter(this MethodInfo method)
    {
        return method.Name.StartsWith("set_");
    }

    public static MethodInfo? GetGetter(this MethodInfo setter)
    {
        if (setter.IsSetter())
        {
            var name = Regex.Replace(setter.Name, "^set_", "get_");
            return setter.DeclaringType!.GetMethod(name);
        }
        else
        {
            return null;
        }
    }

    public static Type? GetClassWithMainMethod()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var classWithMainMethod = entryAssembly?.EntryPoint?.DeclaringType;
        return classWithMainMethod;
    }
}
