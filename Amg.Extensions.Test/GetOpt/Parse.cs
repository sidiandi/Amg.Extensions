using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amg.GetOpt.Test;

public class ParseTests : TestBase
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    [Test]
    public void Parse()
    {
        var args = new[] { "--name", "Alice", "-h", "--long-option=value", "hello", "-vq", "--", "--not-an-option" };
        var o = new WithStandardOptions(new TestCommandObject());
        var parser = new Parser(new CommandProviderImplementation(o).Log(Logger));
        parser.Parse(args);
        Assert.That(true);
    }

    [Test]
    public async Task Parse2()
    {
        var args = new[] { "add", "1", "--name", "Alice", "2", "subtract", "3", "2" };
        var o = new TestCommandObject();
        var parser = new Parser((new CommandProviderImplementation(o)).Log(Logger));
        parser.Parse(args);
        var result = await parser.Run();
        Logger.Information("{@result}", result);
        Assert.That(parser.Operands.Count() == 6);
    }

    [Test]
    public async Task LongOptionWithEqualSign()
    {
        var o = await TestParse(new[] { "--fruit=apple" });
        Assert.That(o.Fruit, Is.EqualTo(Fruit.Apple));
    }

    [Test]
    public async Task LongOptionWithSeparateValue()
    {
        var o = await TestParse(new[] { "--fruit", "o" });
        Assert.That(o.Fruit, Is.EqualTo(Fruit.Orange));
    }

    [Test]
    public async Task OptionStop()
    {
        var o = await TestParse(new[] { "--name", "Alice", "takes-string", "--", "--name" });
        Assert.That(o.Value, Is.EqualTo("--name"));
    }

    static async Task<TestCommandObject> TestParse(string[] args)
    {
        var o = new TestCommandObject();
        var parser = new Parser((new CommandProviderImplementation(o)).Log(Logger));
        parser.Parse(args);
        var result = await parser.Run();
        Logger.Information("{@result}", result);
        return o;
    }

    [Test]
    public void CommandLineException()
    {
        Check(new[] { "-v", "unknown-enum-value" });
        Check(new[] { "-n" });
        Check(new[] { "--name" });
        Check(new[] { "command-does-not-exist" });
        Check(new[] { "--option-does-not-exist" });
        Check(new[] { "add", "1" });
        Check(new[] { "add", "a", "b" });
    }

    static void Check(string[] args)
    {
        try
        {
            var o = new TestCommandObject();
            var parser = new Parser((new CommandProviderImplementation(o)).Log(Logger));
            parser.Parse(args);
            parser.Run().Wait();
            Assert.Fail();
        }
        catch (AggregateException e)
        {
            var cle = (CommandLineException)e.InnerException!;
            Logger.Information("{e}", cle);
        }
        catch (CommandLineException cle)
        {
            Logger.Information("{e}", cle);
        }
    }
}
