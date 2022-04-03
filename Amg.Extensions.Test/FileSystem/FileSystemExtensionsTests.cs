﻿using Amg.Extensions;
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
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    [Test]
    public async Task EnsureDirectoryIsEmpty()
    {
        var testDir = CreateTestDirectory();
        await ChildProcess.Run("git.exe", new[] { "init", testDir });
        Assert.That(testDir.EnumerateFileSystemEntries().Any());
        testDir.EnsureDirectoryIsEmpty();
        Assert.That(testDir.EnumerateFileSystemEntries().Any(), Is.Not.True);
    }

    [Test]
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

        var time = Enumerable.Range(0, 3)
            .Select(_ => MeasureTime(() => source.CopyTree(dest, useHardlinks: useHardlinks)))
            .ToList();
        Logger.Information("{0}", time.Select(_ => new { _.TotalSeconds }).ToTable());
        var sourceVersion = (await FileVersion.Get(source))!;
        var destVersion = (await FileVersion.Get(dest))!;
        destVersion.Name = sourceVersion.Name;
        Assert.That(destVersion, Is.EqualTo(sourceVersion));
    }

    [Test, TestCase(true), TestCase(false)]
    public async Task CopyTreeSingleFile(bool useHardlinks)
    {
        var testDir = CreateTestDirectory();
        Logger.Information(testDir);
        var source = testDir.Combine("source");

        var dest = testDir.Combine("dest");

        await SetupFile(source);

        var time = Enumerable.Range(0, 3)
            .Select(_ => MeasureTime(() => source.CopyTree(dest, useHardlinks: useHardlinks)))
            .ToList();

        Logger.Information("{0}", time.Select(_ => new { _.TotalSeconds }).ToTable());
        Assert.That(await source.IsContentEqual(dest));
    }

    [Test]
    public async Task Hardlinks()
    {
        var testDir = CreateTestDirectory();
        var source = await
            testDir.Combine("original.txt")
            .WriteAllTextAsync("hello");

        var dest = source.CreateHardlink(testDir.Combine("copy.txt"));

        Assert.That(dest.IsFile());

        var info = dest.HardlinkInfo();
        Assert.That(info.HardLinks.Count(), Is.EqualTo(2));

        source.EnsureFileNotExists();
        info = dest.HardlinkInfo();
        Assert.That(info.HardLinks.SequenceEqual(new[] { dest }));
    }

    [Test]
    public void SplitAndCombine()
    {
        var p = @"C:\temp\some\long\path\with\directories\hello.txt";
        Assert.AreEqual(p, p.SplitDirectories().CombineDirectories());
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
        var d = @"C:\a\b\c";
        var f = d.Combine("d", "e", "f");
        Assert.That(d.Combine(f.RelativeTo(d)), Is.EqualTo(f));
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
    public async Task Split()
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
