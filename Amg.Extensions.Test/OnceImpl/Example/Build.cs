namespace Amg.OnceImpl.Example;

internal class Build
{
    public virtual async Task Compile()
    {
        Console.WriteLine("Compiling...");
        await Task.Delay(200);
    }

    public virtual async Task Test()
    {
        await Compile();
        Console.WriteLine("Testing...");
        await Task.Delay(200);
    }

    public virtual async Task Package()
    {
        await Compile();
        Console.WriteLine("Packaging the compiled binaries...");
        await Task.Delay(200);
    }

    public virtual async Task Release()
    {
        await Task.WhenAll(Test(), Package());
        Console.WriteLine("Release complete.");
    }
}
