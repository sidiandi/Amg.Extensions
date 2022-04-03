namespace Amg.GetOpt.Test;

internal class WithDefaultCommand
{
    public WithDefaultCommand()
    {
    }

    [Default, Description("Add two integers.")]
    public int DoSomething(int a, int b)
    {
        result = a + b;
        return result;
    }

    public int result = 0;
}

internal class WithDefaultCommandNoParameters
{
    [Default, Description("do something")]
    public void DoSomething()
    {
        done = true;
    }

    public bool done = false;
}
