using System.ComponentModel;

namespace Amg.GetOpt.Test;

[Description("Test command object")]
internal class TestCommandObject
{
    [Description("add two numbers")]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [Description("subtract two numbers")]
    public int Subtract(int a, int b)
    {
        return a - b;
    }

    [Description("name to be greeted")]
    public string? Name { get; set; }

    [Short('h'), Description("Show help")]
    public bool Help { get; set; }

    [Description("Option with long name")]
    public string? LongOption { get; set; }

    [Short('o'), Description("Enum option")]
    public Fruit Fruit { get; set; }

    [Description("Say hello")]
    public string Greet()
    {
        return (Name == null)
            ? "Hello"
            : $"Hello, {Name}";
    }

    [Description("Method that takes a string")]
    public void TakesString(string value)
    {
        Value = value;
    }

    public string? Value { get; set; }

    [Default, Description("do nothing")]
    public void Default()
    {
        // nothing
    }
}

enum Fruit
{
    Apple,
    Orange,
    Pear
}
