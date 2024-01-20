using Castle.DynamicProxy;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Amg.OnceImpl;

internal class Container
{
    /// <summary>
    /// Default instance
    /// </summary>
    public static Container Instance { get; } = new Container();

    public Container()
    {
        waitUntilCancelled = Task.Delay(-1, cancelAll.Token);
    }

    /// <summary>
    /// Get the property info for a getter or setter method.
    /// </summary>
    /// <param name="getterOrSetterMethod"></param>
    /// <returns>property info, or null if property info for the passed method does not exist.</returns>
    internal static PropertyInfo? GetPropertyInfo(MethodInfo getterOrSetterMethod)
    {
        if (!getterOrSetterMethod.IsSpecialName) return null;
        return getterOrSetterMethod.DeclaringType!.GetProperty(getterOrSetterMethod.Name.Substring(4),
          BindingFlags.Instance |
          BindingFlags.Static |
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
          BindingFlags.NonPublic |
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
          BindingFlags.Public);
    }

    static ProxyGenerator generator = new ProxyGenerator(new DefaultProxyBuilder());

    class InvocationSource : IInvocationSource
    {
        public InvocationSource(IEnumerable<IInvocation> invocations)
        {
            Invocations = invocations;
        }

        public IEnumerable<IInvocation> Invocations { get; private set; }
    }

    /// <summary>
    /// Get an instance of type that executes methods marked with [Once] only once and caches the result.
    /// </summary>
    /// <returns></returns>
    public T Get<T>(params object?[] ctorArguments) => (T)Get(typeof(T), ctorArguments);

    /// <summary>
    /// Get an instance of type that executes methods marked with [Once] only once and caches the result.
    /// </summary>
    /// <returns></returns>
    public object Get(Type type, params object?[] ctorArguments)
    {
        var interceptor = new Interceptor(type, waitUntilCancelled);

        var options = new ProxyGenerationOptions
        {
            Hook = new Hook(),
        };
        options.AddMixinInstance(new InvocationSource(interceptor.Invocations));

        return generator.CreateClassProxy(
            type,
            options,
            ctorArguments,
            interceptor);
    }

    public void CancelAll()
    {
        cancelAll.Cancel();
    }

    readonly CancellationTokenSource cancelAll = new();
    readonly Task waitUntilCancelled;
}
