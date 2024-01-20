using Amg.Extensions;
using Amg.OnceImpl;

namespace Amg;

/// <summary>
/// Create objects that execute methods marked with [Once] only once
/// </summary>
public static class Once
{
    /// <summary>
    /// Get an instance of type that executes all methods only once and caches the result.
    /// </summary>
    /// Conditions for Type:
    /// * all instance methods are virtual
    /// * all constructors must be public
    /// * all fields readonly
    /// <returns>Type instance with "once" behaviour</returns>
    public static object Create(Type type, params object?[] ctorArguments)
    {
        return Container.Instance.Get(type, ctorArguments);
    }

    /// <summary>
    /// Get an instance of T that executes methods marked with [Once] only once and caches the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Create<T>(params object?[] ctorArguments) where T : class
    {
        return (T)Create(typeof(T), ctorArguments);
    }

    public static IWritable Timeline(object once)
    {
        var source = (once as IInvocationSource)!;
        return Amg.OnceImpl.Summary.PrintTimeline(source.Invocations);
    }
}
