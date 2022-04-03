using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Amg.OnceImpl;

[Serializable]
public class PropertyCanOnlyBeSetBeforeFirstGetException : Exception
{
    public MethodInfo Method { get; }

    public PropertyCanOnlyBeSetBeforeFirstGetException(MethodInfo method)
    : base($"Property {method} is decorated with [Once] and can only be set once.")
    {
        this.Method = method;
    }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    protected PropertyCanOnlyBeSetBeforeFirstGetException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
    }
}
