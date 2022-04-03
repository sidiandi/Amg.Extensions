using Amg.GetOpt;
using System;
using System.ComponentModel;

namespace example;

public class Program
{
    static int Main(string[] args) => Amg.Program.Once();

    [Description("Add two numbers.")]
    public virtual int Add(int a, int b)
    {
        return a + b;
    }

    [Description("Greet the world.")]
    public virtual void Greet()
    {
        Console.WriteLine($"Hello, {Name}.");
    }

    [Short('n'), Description("Name to greet")]
    public virtual string Name { get; set; } = "world";
}
