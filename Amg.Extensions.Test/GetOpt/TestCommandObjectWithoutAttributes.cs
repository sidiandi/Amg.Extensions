namespace Amg.GetOpt.Test;
internal class TestCommandObjectWithoutAttributes
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    public string? Name { get; set; }

    public bool Help { get; set; }

    public string? LongOption { get; set; }

    public Fruit Fruit { get; set; }

    public string Greet()
    {
        return (Name == null)
            ? "Hello"
            : $"Hello, {Name}";
    }

    public void TakesString(string value)
    {
        Value = value;
    }

    public string? Value { get; set; }
}
