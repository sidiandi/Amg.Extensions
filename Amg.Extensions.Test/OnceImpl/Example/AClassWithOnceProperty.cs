namespace Amg.OnceImpl.Example;
#pragma warning restore CS0414

public class AClassWithOnceProperty
{
    public virtual string? Name { get; set; } = null;

    public virtual string Greet()
    {
        return $"Hello, {Name}!";
    }
}
