using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Amg.OnceImpl;

/// <summary>
/// Creates proxies for classes that execute methods only once.
/// </summary>

internal interface IInvocationSource
{
    IEnumerable<IInvocation> Invocations { get; }
}
