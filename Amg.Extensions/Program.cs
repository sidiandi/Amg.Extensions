namespace Amg;

public static class Program
{
    public static int Once() =>
        AutoBuild.Main(
            main: _ => Amg.GetOpt.GetOpt.Main(_, 
                Amg.Once.Create(Amg.Extensions.SystemExtensions.GetClassWithMainMethod()!)));

    public static int GetOpt()
        => Amg.GetOpt.GetOpt.Main(
            System.Environment.GetCommandLineArgs().Skip(1).ToArray());
}
