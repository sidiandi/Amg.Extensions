using System;
using System.Reflection;
using System.Runtime.Serialization;

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
