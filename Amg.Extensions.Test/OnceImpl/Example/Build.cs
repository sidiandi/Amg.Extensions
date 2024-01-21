namespace Amg.OnceImpl.Example;

internal class Build
{
    public virtual async Task Compile()
    {
        // Compiling...
        await Task.Delay(200);
    }

    public virtual async Task Test()
    {
        await Compile();
        // Testing...
        await Task.Delay(200);
    }

#pragma warning disable S4144 // Methods should not have identical implementations
    public virtual async Task Package()
#pragma warning restore S4144 // Methods should not have identical implementations
    {
        await Compile();
        // Packaging the compiled binaries...
        await Task.Delay(200);
    }

    public virtual async Task Release()
    {
        await Task.WhenAll(Test(), Package());
        // Release complete.
    }
}
