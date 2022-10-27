using Amg.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Amg.FileSystem;

[TestFixture]
public class FileSystemExtensionsTests
{
    private static readonly Serilog.ILogger Logger = Logger();

    [Test]
    public async Task EnsureDirectoryIsEmpty()
    {
        var testDir = CreateTestDirectory();
        await ChildProcess.Run("git", new[] { "init", testDir });
        Assert.That(testDir.EnumerateFileSystemEntries().Any());
        testDir.EnsureDirectoryIsEmpty();
        Assert.That(testDir.EnumerateFileSystemEntries().Any(), Is.Not.True);
    }

    [Test]
    [Platform("Win")]
    public async Task MoveToRecyclingBin()
    {
        var testDir = CreateTestDirectory();
        var repoDir = testDir.Combine("repo");
        await ChildProcess.Run("git.exe", new[] { "init", repoDir });
        Assert.That(repoDir.IsDirectory());
        Assert.That(repoDir.EnumerateFileSystemEntries().Any());
        testDir.EnsureDirectoryIsEmpty();
        Assert.That(repoDir.Exists(), Is.Not.True);
    }

    [Test]
    public void ParentDirectory()
    {
        var testDir = CreateTestDirectory();
        var d = testDir.Combine("a", "b");
        var f = d.Combine("c");
        f.EnsureParentDirectoryExists();
        Assert.That(Directory.Exists(d));
        Assert.That(f.Parent(), Is.EqualTo(d));
    }

    [Test]
    public void LastModified()
    {
        var dir = ".";
        var t = dir.Glob("**/*").LastWriteTimeUtc();
        Assert.That(t, Is.Not.EqualTo(default(DateTime)));
    }

    static string GetThisSourceFile([CallerFilePath] string? path = null) => path!;

    [Test]
    public void OutOfDate()
    {
        var thisDll = Assembly.GetExecutingAssembly().Location;
        var sources = GetThisSourceFile().Parent()
            .Glob("**")
            .Exclude("obj")
            .Exclude("bin");

        Assert.That(thisDll.IsOutOfDate(sources), Is.Not.True);
    }

    bool IsValidFilename(string f)
    {
        try
        {
            var d = CreateTestDirectory();
            var p = d.Combine(f);
            p.WriteAllTextAsync("a").Wait();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Test]
    [Platform("Win")]
    public void Combine()
    {
        var combined = @"C:\temp\a\b\c";
        Assert.That(@"C:\temp".Combine("a", "b", "c"), Is.EqualTo(combined));
        Assert.That(@"C:\temp".Combine(@"a\b", "c"), Is.EqualTo(combined));
        Assert.That(@"C:\temp".Combine(@"a\b\c"), Is.EqualTo(combined));
        Assert.That(@"C:\temp".Combine(@"a/b/c"), Is.EqualTo(combined));
    }

    [Test]
    public void CombineChecksForValidFileNames()
    {
        var e = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            @"C:\temp".Combine("a", "b", new string('c', 1024));
        });
        Logger.Information("{exception}", e);
    }

    [Test]
    [Platform("Win")]
    public void MakeValidFilename()
    {
        Assert.That("x".MakeValidFileName(), Is.EqualTo("x"));

        var invalid = new string(Path.GetInvalidFileNameChars());
        Assert.That(invalid.MakeValidFileName(), Is.EqualTo(new string('_', invalid.Length)));

        var tooLong = new string('a', 1024) + ".ext";
        Assert.That(tooLong.IsValidFileName(), Is.False);
        var shortened = tooLong.MakeValidFileName();
        Assert.That(shortened.IsValidFileName(), Is.True);
        Logger.Information("{shortened}", shortened);
        Assert.That(IsValidFilename(shortened));
        Assert.That(shortened.Extension(), Is.EqualTo(tooLong.Extension()));
    }

    [Test]
    public void MakeValidPosixFilename()
    {
        Assert.That("x".MakeValidFileName(), Is.EqualTo("x"));

        var tooLong = new string('a', 1024) + ".ext";
        Assert.That(tooLong.IsValidPosixFileName(), Is.False);
        var shortened = tooLong.MakeValidPosixFileName();
        Assert.That(shortened.IsValidPosixFileName(), Is.True);
        Logger.Information("{shortened}", shortened);
        Assert.That(shortened.Extension(), Is.EqualTo(tooLong.Extension()));
    }

    [Test]
    public void MakeShortValidPosixFilename()
    {
        var tooLong = "Hello World, this is a slightly too long file name.ext";
        var maxLength = 16;
        var s = tooLong.MakeValidPosixFileName(maxLength);
        Assert.That(s.IsValidPosixFileName());
        Assert.That(s.Length, Is.EqualTo(maxLength));
        Assert.That(s, Is.EqualTo("a54da7b29720.ext"));
        Logger.Information("{shortened}", s);
    }

    [Test]
    public void ChangeRoot()
    {
        Assert.AreEqual(@"C:\newRoot\a\b\c\d", @"C:\oldRoot\a\b\c\d".ChangeRoot(@"C:\oldRoot", @"C:\newRoot"));
    }

    static string TestFileName(int i)
    {
        return i.ToString("X").Select(_ => new string(_, 1)).Join(@"\") + ".txt";
    }

    static async Task SetupTree(string root)
    {
        var files = Enumerable.Range(0, 100).Select(_ => root.Combine(TestFileName(_))).ToList();
        foreach (var i in files)
        {
            await i.WriteAllTextAsync(new string('a', 100 * 1024));
        }
    }

    static async Task SetupFile(string root)
    {
        await root.WriteAllTextAsync("hello");
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task CopyTree(bool useHardlinks)
    {
        var testDir = CreateTestDirectory();
        Logger.Information(testDir);
        var source = testDir.Combine("source");
        await SetupTree(source);

        var dest = testDir.Combine("dest");

        await source.CopyTree(dest, useHardlinks: useHardlinks);
        var sourceVersion = (await FileVersion.Get(source))!;
        var destVersion = (await FileVersion.Get(dest))!;
        destVersion.Name = sourceVersion.Name;
        Assert.That(destVersion, Is.EqualTo(sourceVersion));
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task CopyTreeSingleFile(bool useHardlinks)
    {
        var testDir = CreateTestDirectory();
        var source = testDir.Combine("source");
        var dest = testDir.Combine("dest");
        await SetupFile(source);

        await source.CopyTree(dest, useHardlinks);

        Assert.That(await source.IsContentEqual(dest));
    }

    [Test]
    [Platform("Win")]
    public async Task Hardlinks()
    {
        var testDir = CreateTestDirectory();
        var source = await
            testDir.Combine("original.txt")
            .WriteAllTextAsync("hello");

        var dest = await source.CreateHardlink(testDir.Combine("copy.txt"));

        Assert.That(dest.IsFile());

        var info = await dest.HardlinkInfo();
        Assert.That(info.HardLinks.Count(), Is.EqualTo(2));

        source.EnsureFileNotExists();
        info = await dest.HardlinkInfo();
        Assert.That(info.HardLinks.SequenceEqual(new[] { dest }));
    }

    [Test]
    public void SplitAndCombine()
    {
        var p = @"C:\temp\some\long\path\with\directories\hello.txt";
        Assert.AreEqual(p, p.SplitDirectories().CombineDirectories());
    }

    [Test]
    [Platform("Win")]
    public void SplitDirectoriesUnc()
    {
        var p = @"\\server\share$\some\long\path\with\directories\hello.txt";
        var d = p.SplitDirectories();
        Assert.That(d[0], Is.EqualTo(@"\\server\share$"));
        Assert.AreEqual(p, d.CombineDirectories());
    }

    [Test]
    [TestCase(@"C:\a\b\c", false)]
    [TestCase(@"\\server\share\a\b\c", true)]
    [TestCase(@"\\?\C:\File", false)]
    public void IsUnc(string path, bool isUnc)
    {
        Assert.That(path.IsUnc(), Is.EqualTo(isUnc));
    }

    [Test]
    public void DateTimeToFileName()
    {
        var time = new DateTime(2019, 10, 5, 3, 31, 12, 234);
        var fn = time.ToFileName();
        Assert.That(fn.IsValidFileName());
        Assert.AreEqual("2019-10-05T03_31_12.2340000", fn);
    }

    [Test]
    public void DateTimeToShortFileName()
    {
        var time = new DateTime(2019, 10, 5, 3, 31, 12, 234);
        var fn = time.ToShortFileName();
        Assert.That(fn.IsValidFileName());
        Assert.AreEqual("4U8QD6UITWO0", fn);

        time = DateTime.MaxValue;
        fn = time.ToShortFileName();
        Assert.That(fn.IsValidFileName());
        Assert.AreEqual("NZ14HU5JI7ZZ", fn);
    }

    [Test]
    public void PathEqualsIsCaseSensitiveDependingOnOsPlaform()
    {
        if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            Assert.That(@"A.TXT".EqualsPath("a.txt"));
        }
        else if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
        {
            Assert.That(!@"A.TXT".EqualsPath("a.txt"));
        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public async Task NotExisting()
    {
        var testDir = CreateTestDirectory();
        var a = await testDir.Combine("a").Touch();
        var a0 = a.GetNotExisting();
        Assert.That(!a0.Exists());
        Assert.That(a0.Parent(), Is.EqualTo(a.Parent()));
        Assert.That(a0.FileName(), Does.StartWith(a.FileName()));
    }

    [Test]
    public async Task Touch()
    {
        var testDir = CreateTestDirectory();
        var p = await testDir.Combine("a", "b", "c").Touch();
        var t = p.Info()!.LastWriteTimeUtc;
        Assert.That(p.Exists());
        await Task.Delay(100);
        await testDir.Combine("a", "b", "c").Touch();
        Assert.That(p.Exists());
        Assert.That(p.Info()!.LastAccessTimeUtc, Is.Not.EqualTo(t));
    }

    [Test]
    public void RelativeTo()
    {
        var b = @"C:\a\b\c";
        var p = b.Combine("d", "e", "f");
        var r = p.RelativeTo(b);
        Assert.That(b.Combine(r), Is.EqualTo(p));
    }

    [Test]
    public void RelativeToIsCaseInsensitiveOnWindows()
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) Assert.Pass();

        var d = @"C:\a\b\c";
        var f = @"C:\A\b\c\d\e\f";
        Assert.That(f.RelativeTo(d), Is.EqualTo(@"d\e\f"));
    }

    [Test]
    public void RelativeToWithDots()
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) Assert.Pass();

        var d = @"C:\a\b\c";
        var f = @"C:\a\g\h\i\j";
        Assert.That(f.RelativeTo(d), Is.EqualTo(@"..\..\g\h\i\j"));
    }

    [Test]
    public void RelativeTrailingSlash()
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) Assert.Pass();

        var b = @"C:\a\b\c\";
        var f = @"C:\a\g\h\i\j";
        var r = f.RelativeTo(b);
        Assert.That(b.Combine(r).Canonical(), Is.EqualTo(f));
        Assert.That(f.RelativeTo(b), Is.EqualTo(@"..\..\g\h\i\j"));
    }

    [Test]
    public void RelativeToWithDots2()
    {
        if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) Assert.Pass();

        var d = @"\\server\share\a\b\c";
        var f = @"\\server\othershare\j";
        Assert.Throws<System.ArgumentOutOfRangeException>(() => f.RelativeTo(d));
    }

    [Test]
    [TestCase(@"C:\", @"C:\")]
    [TestCase(@"C:\a\b\c\", @"C:\a\b\c")]
    [TestCase(@"C:\a\b\c\.", @"C:\a\b\c")]
    [TestCase(@"C:\a\b\c\d\..\.", @"C:\a\b\c")]
    [TestCase(@"C:\a\b\c\d\..\.", @"C:\a\b\c")]
    [TestCase(@"C:\a\b\c\d\..\..\..\f", @"C:\a\f")]
    [TestCase(@"\\server\share$\a\b\c\d\", @"\\server\share$\a\b\c\d")]
    [Platform("Win")]
    public void Canonical(string path, string expectedCanonicalPath)
    {
        Assert.That(path.Canonical(), Is.EqualTo(expectedCanonicalPath));
    }

    [Test]
    public void CurrentDirectoryToCanonical()
    {
        var c = ".".Canonical();
        var currentDirectory = System.Environment.CurrentDirectory;
        Assert.That(c.EqualsPath(currentDirectory), () =>
            $"c={c}, currentDirectory={currentDirectory}"
        );
    }

    [Test]
    public void DescendantOrSelf()
    {
        var a = @"C:\a\b\c";
        var b = a.Combine("d");
        var c = a.Parent();

        Assert.That(a.IsDescendantOrSelf(a));
        Assert.That(b.IsDescendantOrSelf(a));
        Assert.That(!c.IsDescendantOrSelf(a));
    }

    [Test]
    public async Task SplitFile()
    {
        var testDir = CreateTestDirectory();
        var count = 100;
        var content = "hello";
        var input = await testDir.Combine("whole")
            .WriteAllTextAsync(Enumerable.Range(0, count).Select(_ => content).Join(String.Empty));
        var output = testDir.Combine("part");

        var parts = await input.SplitFile(output, content.Length);
        Assert.That(parts.Count(), Is.EqualTo(count));
        Assert.That(parts.All(_ => _.IsFile()));
    }
}
