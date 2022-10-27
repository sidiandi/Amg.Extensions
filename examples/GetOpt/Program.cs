using Amg.GetOpt;
using Serilog;
using System;
using System.ComponentModel;

namespace example;

public class Other
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

#pragma warning disable S1118 // Utility classes should not have public constructors
    public Other()
#pragma warning restore S1118 // Utility classes should not have public constructors
    {
        Logger.Information("Other ctor");
    }
}

public class Program
{
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
#pragma warning disable S2933 // Fields that are only assigned in the constructor should be "readonly"
    private Serilog.ILogger Logger;
#pragma warning restore S2933 // Fields that are only assigned in the constructor should be "readonly"
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

    static int Main(string[] args) => Amg.Program.GetOpt();

    public Program()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Logger = Serilog.Log.Logger.ForContext<Program>();
    }

    [Description("Add two numbers.")]
    public int Add(int a, int b)
    {
        Logger.Information("ctor");
#pragma warning disable S1481 // Unused local variables should be removed
        var o = new Other();
#pragma warning restore S1481 // Unused local variables should be removed
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
