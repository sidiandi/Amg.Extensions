using Amg.GetOpt;
using System;
using System.ComponentModel;

namespace example;

class Program
{
    static void Main(string[] args) => Amg.GetOpt.GetOpt.Main(args, new Program());

    [Description("Add two numbers.")]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [Description("Greet the world.")]
    public void Greet()
    {
        Console.WriteLine($"Hello, {Name}.");
    }

    [Short('n'), Description("Name to greet")]
    public string Name { get; set; } = "world";
}
