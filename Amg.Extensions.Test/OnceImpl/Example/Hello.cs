using System.Collections.Generic;
using System.Net.Http;

namespace Amg.OnceImpl.Example;

public class Hello
{
    public virtual string Name { get; set; }

    protected Hello(string name)
    {
        Name = name;
    }

    public virtual void Greet()
    {
        Count.Enqueue(0);
    }

    public virtual string Greeting => $"Hello, {Name}";

    public virtual HttpClient Web => new HttpClient();

    public readonly Queue<int> Count = new ();
}
