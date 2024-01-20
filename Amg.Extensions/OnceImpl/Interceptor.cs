using System.Reflection;
using System.Threading.Tasks;
using Amg.Collections;
using Amg.Extensions;
using Castle.DynamicProxy;

namespace Amg.OnceImpl;

internal class Interceptor : IInterceptor
{
    public Interceptor(Type type, Task waitUntilCancelled)
    {
        this.type = type;
        this.waitUntilCancelled = waitUntilCancelled;
        this.cache = new Dictionary<InvocationId, IInvocation>();
    }

    public string CacheDir = @".\.cache";

    public Task waitUntilCancelled { get; }

    readonly IDictionary<InvocationId, IInvocation> cache;
    private readonly Type type;

    public IEnumerable<IInvocation> Invocations => cache.Values;

    public void Intercept(Castle.DynamicProxy.IInvocation invocation)
    {
        if (!type.Equals(invocation.Method.DeclaringType))
        {
            invocation.Proceed();
            return;
        }

        var cacheKey = new InvocationId(invocation.Method, invocation.Arguments);

        if (invocation.Method.IsSetter())
        {
            var getterId = cacheKey.GetGetterId();
            if (getterId is { })
            {
                if (cache.ContainsKey(getterId))
                {
                    throw new PropertyCanOnlyBeSetBeforeFirstGetException(invocation.Method);
                }

                invocation.ReturnValue = CreateInvocation(this, cacheKey, invocation).ReturnValue;
            }
        }
        else
        {
            invocation.ReturnValue = cache.GetOrAdd(
                cacheKey, 
                () => CreateInvocation(this, cacheKey, invocation))
                .ReturnValue;
        }
    }

    static IInvocation CreateInvocation(
        Interceptor interceptor,
        InvocationId id,
        Castle.DynamicProxy.IInvocation invocation)
    {
        if (invocation.Method.GetCustomAttribute<CachedAttribute>() is { })
        {
            return new CachedInvocationInfo(interceptor, id, invocation);
        }
        else
        {
            return new InvocationInfo(interceptor, id, invocation);
        }
    }
}
