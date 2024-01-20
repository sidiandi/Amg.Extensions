using System.Reflection;

namespace Amg.OnceImpl;

public class PropertyCanOnlyBeSetBeforeFirstGetException : Exception
{
    public MethodInfo Method { get; }

    public PropertyCanOnlyBeSetBeforeFirstGetException(MethodInfo method)
    : base($"Property {method} is decorated with [Once] and can only be set once.")
    {
        this.Method = method;
    }
}
