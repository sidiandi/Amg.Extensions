namespace Amg.GetOpt.Test;

[TestFixture]
public class CommandProviderTests
{

    [Test]
    public void DiscoverCommandsAndOptions()
    {
        var x = new ClassWithCommandAndOption();
        var p = new CommandProviderImplementation(x);
        AssertCommands(p);
    }

    static void AssertCommands(ICommandProvider p)
    {
        Assert.That(p.Commands.Count(), Is.EqualTo(1));
        var c = p.Commands.First();
        Assert.That(c.Name, Is.EqualTo("command"));
        Assert.That(p.Options.Count(), Is.EqualTo(1));
        var o = p.Options.First();
        Assert.That(o.Long, Is.EqualTo("option"));
        Assert.That(o.Short, Is.EqualTo("o"));
    }

    [Test]
    public void ComposeCommandsAndOptions()
    {
        var x = new ClassThatComposesCommandAndOption();
        var p = new CommandProviderImplementation(x);
        AssertCommands(p);
    }

    [Test]
    public void IsCommand()
    {
        var m = typeof(StandardOptions).GetMethod("OnOptionsParsed")!;
        Assert.That(CommandProviderImplementation.IsCommand(m), Is.False);
    }

    [Test]
    public void HasDescriptionAttributes()
    {
        var t = typeof(StandardOptions);
        Assert.That(CommandProviderImplementation.HasDescriptionAttributes(t), Is.True);
    }
}
