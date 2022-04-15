using Amg.Extensions;
using Amg.FileSystem;
using Amg.OnceImpl.Example;
using Serilog;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Amg.OnceImpl;

[TestFixture]
public class OnceTests
{
    private static readonly Serilog.ILogger Logger = Logger();

    public class MethodIsOnlyCalledOnceTestClass
    {
        public virtual void A()
        {
            calls.Push("A");
        }

        public readonly Stack<string> calls = new();
    }

    [Test]
    public void MethodIsOnlyCalledOnce()
    {
        var a = Amg.Once.Create<MethodIsOnlyCalledOnceTestClass>();
        a.A();
        a.A();
        Assert.That(a.calls.Count, Is.EqualTo(1));
    }

    [SetUp]
    public void SetUp()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    [Test]
    public async Task RunOnce()
    {
        var once = Amg.Once.Create<MyBuild>();
        await once.All();
        Console.WriteLine(Amg.Once.Timeline(once));
        Assert.That(once.result.Distinct().Count() == once.result.Count);
    }

    [Test]
    public void OnceCannotBeAppliedWhenClassHasMutableFields()
    {
        Assert.Throws<OnceException>(() =>
        {
            Amg.Once.Create<AClassThatHasMutableFields>();
        });
    }

    [Test]
    public void OncePropertiesWithSettersCanOnlyBeSetOnce()
    {
        var once = Once.Create<AClassWithOnceProperty>();
        once.Name = "Alice";
        once.Name = "Bob";
        Assert.That(once.Name, Is.EqualTo("Bob"));

        Assert.Throws<PropertyCanOnlyBeSetBeforeFirstGetException>(() =>
        {
            once.Name = "Bob";
        });
    }

    [Test]
    public void OnceInstanceCanBeConfiguredWithPublicProperties()
    {
        var once = Once.Create<AClassWithOnceProperty>();
        once.Name = "Alice";
        Assert.That(once.Greet(), Is.EqualTo("Hello, Alice!"));

        Assert.Throws<PropertyCanOnlyBeSetBeforeFirstGetException>(() =>
        {
            once.Name = "Bob";
        });
    }

    [Test]
    public void OnlyExecutesOnce()
    {
        var name = "Alice";
        var hello = Once.Create<Hello>(name);
        hello.Greet();
        hello.Greet();
        Assert.That(hello.Count.Count, Is.EqualTo(1));
    }

    [Test]
    public void OnlyExecutesOnceNoAttributes()
    {
        var name = "Alice";
        var hello = Once.Create<AClassWithoutOnceAttributes>(name);
        hello.Greet();
        hello.Greet();
        Assert.That(hello.Count.Count, Is.EqualTo(1));
    }

    public class CachedExample
    {
        [Cached]
        public virtual Task<IEnumerable<long>> GetFiles(string dir) => Task.Factory.StartNew(() =>
        {
            return new DirectoryInfo(dir).EnumerateFileSystemInfos("*.*", SearchOption.AllDirectories)
                .SafeSelect(_ => (_ as FileInfo)?.Length ?? 0);
        });

        public async virtual Task<long> Size()
        {
            var files = GetFiles(Assembly.GetExecutingAssembly().Location.Parent());
            var files2 = GetFiles(Assembly.GetExecutingAssembly().Location.Parent());
            return (await files).Concat((await files2)).Sum(_ => _);
        }
    }

    [Test]
    public async Task Cached()
    {
        var dir = CreateTestDirectory();
        System.Environment.CurrentDirectory = dir;
        var f = Once.Create<CachedExample>();
        var sw0 = Stopwatch.StartNew();
        var size0 = await f.Size();
        var d0 = sw0.Elapsed;
        var f1 = Once.Create<CachedExample>();
        var sw1 = Stopwatch.StartNew();
        var size1 = await f1.Size();
        var d1 = sw1.Elapsed;
        Console.WriteLine(new { size0, size1, d0, d1 });
        Assert.That(d1 < d0);
    }
}