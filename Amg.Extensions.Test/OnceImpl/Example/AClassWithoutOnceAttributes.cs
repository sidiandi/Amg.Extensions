using System.Collections.Generic;
using System.Net.Http;

namespace Amg.OnceImpl.Example;

public class AClassWithoutOnceAttributes
{
    public virtual string Name { get; }

    protected AClassWithoutOnceAttributes(string name)
    {
        Name = name;
    }

    public virtual void Greet()
    {
        Count.Enqueue(0);
    }

    public virtual string Greeting => $"Hello, {Name}";

    public virtual HttpClient Web => new HttpClient();

    public virtual Queue<int> Count => new();
}
