using Amg.Extensions;
using System;

namespace Amg.GetOpt.Test;

[TestFixture]
class GetOptTests : TestBase
{
    [Test]
    public void Run()
    {
        var co = new TestCommandObject();
        var exitCode = GetOpt.Main(new[]
        {
                "add",
                "1",
                "--name",
                "Alice",
                "2",
                "subtract",
                "3",
                "2",
                "greet"
            }, co);
        Assert.That(exitCode, Is.EqualTo(0));
        Assert.That(co.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public void RunNoDescriptionAttributes()
    {
        var co = new TestCommandObjectWithoutAttributes();
        var exitCode = GetOpt.Main(new[]
        {
                "add",
                "1",
                "--name",
                "Alice",
                "2",
                "subtract",
                "3",
                "2",
                "greet"
            }, co);
        Assert.That(exitCode, Is.EqualTo(0));
        Assert.That(co.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public void ArgumentMissing()
    {
        var co = new TestCommandObject();
        var exitCode = GetOpt.Main(new[]
        {
                "add",
                "1",
            }, co);
        Assert.That(exitCode, Is.Not.EqualTo(0));
    }

    [Test]
    public void Help()
    {
        var o = new WithStandardOptions(new TestCommandObject());
        var p = new CommandProviderImplementation(o);
        var helpMessage = TextFormatExtensions.GetWritable(_ => Amg.GetOpt.Help.PrintHelpMessage(_, p)).ToString();
        Console.WriteLine(helpMessage);
        Assert.That(helpMessage, Does.Contain("Run a command."));
        Assert.That(helpMessage, Does.Contain("Options:"));
        Assert.Pass(helpMessage);
    }

    [Test]
    public void HelpNoDescriptionAttributes()
    {
        var o = new WithStandardOptions(new TestCommandObjectWithoutAttributes());
        var p = new CommandProviderImplementation(o);
        var helpMessage = TextFormatExtensions.GetWritable(_ => Amg.GetOpt.Help.PrintHelpMessage(_, p)).ToString();
        Console.WriteLine(helpMessage);
        Assert.That(helpMessage, Does.Contain("Run a command."));
        Assert.That(helpMessage, Does.Contain("Options:"));
        Assert.That(helpMessage, Is.EqualTo(@"
usage: testhost [options] <command> [<args>]
Run a command.

Commands:
add <a: int32> <b: int32>
subtract <a: int32> <b: int32>
greet
takes-string <value>

Options:
--fruit <apple|orange|pear>
-h|--help : Print help and exit.
--long-option <string>
--name <string>
--value <string>
-v|--verbosity <quiet|minimal|normal|detailed> : Logging verbosity
--version : Print version and exit.
"));
    }

    [Test]
    public void HelpNoCommands()
    {
        var o = new OnlyDefaultCommand();
        var p = new CommandProviderImplementation(o);
        var helpMessage = TextFormatExtensions.GetWritable(_ => Amg.GetOpt.Help.PrintHelpMessage(_, p)).ToString();
        Assert.Pass(helpMessage);
    }

    [Test]
    public void DefaultCommand()
    {
        var o = new WithDefaultCommand();
        var exitCode = GetOpt.Main(new string[] { "1", "1" }, o);
        Assert.AreEqual(ExitCode.Success, exitCode);
        Assert.That(o.result, Is.EqualTo(2));
    }

    [Test]
    public void HelpForDefaultCommandNoParameters()
    {
        var o = new WithDefaultCommandNoParameters();
        var (output, error) = CaptureOutput(() =>
        {
            var exitCode = GetOpt.Main(new string[] { "-h" }, o);
            Assert.AreEqual(ExitCode.HelpDisplayed, exitCode);
        });
        Console.WriteLine(output);
        Assert.That(!output.Contains("do-something"));
    }

    [Test]
    public void DefaultCommandWithoutParameters()
    {
        var o = new WithDefaultCommandNoParameters();
        int exitCode = 0;
        var (output, error) = CaptureOutput(() =>
        {
            exitCode = GetOpt.Main(new string[] { "1", "1" }, o);
        });
        Console.WriteLine(error);
        Assert.AreEqual(ExitCode.CommandLineError, exitCode);
    }
}
