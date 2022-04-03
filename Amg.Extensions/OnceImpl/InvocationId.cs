using Amg.Extensions;
using System.Reflection;

namespace Amg.OnceImpl;

public sealed class InvocationId : IEquatable<InvocationId>
{
    public InvocationId(MethodInfo method, object?[] arguments)
    {
        Method = method;
        Arguments = arguments;
    }

    readonly MethodInfo Method;
    readonly object?[] Arguments;

    public string Uid => Json.Hash(new { Method.Name, Arguments });

    public override bool Equals(object? obj) => Equals(obj as InvocationId);

    public bool Equals(InvocationId? other)
    {
        return other is { } && this.Method.Equals(other.Method)
            && Arguments.SequenceEqual(other.Arguments);
    }

    public override int GetHashCode()
    {
        return Method.GetHashCode();
    }

    public override string? ToString() => Method.Name;

    public InvocationId GetGetterId()
    {
        var getter = this.Method.GetGetter();
        return new InvocationId(getter!, new object?[] { });
    }
}
