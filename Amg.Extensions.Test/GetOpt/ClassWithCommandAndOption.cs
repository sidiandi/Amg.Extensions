using System.ComponentModel;

namespace Amg.GetOpt.Test;

class ClassWithCommandAndOption
{
    [Description("command")]
    public void Command()
    {
        // nothing here
    }

    [Short('o'), Description("option")]
    public string? Option { get; set; }
}

class ClassThatComposesCommandAndOption
{
    [CommandProvider]
    public ClassWithCommandAndOption Composed { get; } = new ClassWithCommandAndOption();
}
