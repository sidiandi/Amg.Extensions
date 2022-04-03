using System;

namespace example;

class Program
{
    static int Main(string[] args) => Amg.GetOpt.GetOpt.Main(args);

    public int Add(int a, int b)
    {
        return a + b;
    }

    public void Greet()
    {
        Console.WriteLine($"Hello, {Name}.");
    }

    public string Name { get; set; } = "world";
}
