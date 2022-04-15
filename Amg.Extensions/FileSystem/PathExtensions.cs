﻿using Amg.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amg.FileSystem;

/// <summary>
/// Extensions to work with file system objects.
/// </summary>
/// These extensions of `string` allow fluent handling of file and directory paths.
/// For examples, see Amg.Build.Tests/FileSystemExtensionsTests.cs
public static class PathExtensions
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    /// <summary>
    /// Creates the parent directory of path is necessary. 
    /// </summary>
    /// <param name="path"></param>
    /// <returns>path</returns>
    public static string EnsureParentDirectoryExists(this string path)
    {
        path.Parent().EnsureDirectoryExists();
        return path;
    }

    /// <summary>
    /// Ensure that the file given by path does not exist. Deletes also read-only files
    /// </summary>
    /// <param name="path"></param>
    /// <returns>path</returns>
    public static string EnsureFileNotExists(this string path)
    {
        if (File.Exists(path))
        {
            new FileInfo(path).DeleteReadOnly();
        }
        return path;
    }

    /// <summary>
    /// Appends path elements to path.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="pathElements"></param>
    /// <returns>directory with pathElements appended.</returns>
    public static string Combine(this string directory, params string[] pathElements)
    {
        var elements = pathElements.SelectMany(_ => _.SplitDirectories());
        if (elements.Any(_ => !_.IsValidFileName()))
        {
            throw new ArgumentOutOfRangeException(nameof(pathElements), pathElements,
                $@"Invalid file name:

{elements.Select(_ => new { valid = _.IsValidFileName(), fileName = _ }).ToTable(header: true)}");
        }
        return Path.Combine(new[] { directory }.Concat(elements).ToArray());
    }

    /// <summary>
    /// Ensures that dir exists and is empty. Deletes also read-only files
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static string EnsureDirectoryIsEmpty(this string dir)
    {
        if (Directory.Exists(dir))
        {
            foreach (var i in new DirectoryInfo(dir).EnumerateFileSystemInfos())
            {
                i.DeleteReadOnly();
            }
        }
        return dir.EnsureDirectoryExists();
    }

    /// <summary>
    /// Deletes the directory tree fileSystemInfo even if it contains read-only elements.
    /// </summary>
    /// <param name="fileSystemInfo"></param>
    public static void DeleteReadOnly(this FileSystemInfo fileSystemInfo)
    {
        var directoryInfo = fileSystemInfo as DirectoryInfo;
        if (directoryInfo != null)
        {
            foreach (FileSystemInfo childInfo in directoryInfo.GetFileSystemInfos())
            {
                childInfo.DeleteReadOnly();
            }
        }

        fileSystemInfo.Attributes = FileAttributes.Normal;
        fileSystemInfo.Delete();
    }

    /// <summary>
    /// Makes a relative path absolute.
    /// </summary>
    /// <param name="path">Relative or absolute path.</param>
    /// <returns>Absolute path.</returns>
    public static string Absolute(this string path)
    {
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Returns true if path has any of the extensions `extensionWithDots`. Ignores case.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extensionsWithDots"></param>
    /// <returns>True, if path has one of the passed extensions, false otherwise.</returns>
    public static bool HasExtension(this string path, params string[] extensionsWithDots)
    {
        var e = path.Extension();
        return extensionsWithDots.Any(_ => _.Equals(e, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the extension of the path, including the dot (.).
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string Extension(this string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    /// Returns the file name of the path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string FileName(this string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// File name without extension
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string FileNameWithoutExtension(this string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// Ensures that the directory dir exists
    /// </summary>
    /// <param name="dir"></param>
    /// <returns>dir</returns>
    public static string EnsureDirectoryExists(this string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    /// <summary>
    /// Sequence of parent directories, starting with path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerable<string> Up(this string path)
    {
        for (string? i = path.Absolute(); i != null; i = i.ParentOrNull())
        {
            yield return i;
        }
    }

    /// <summary>
    /// Returns the parent directory. Throws, if no parent directory exists for path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string Parent(this string path)
    {
        var p = path.ParentOrNull();
        if (p == null)
        {
            throw new ArgumentOutOfRangeException(nameof(path), path, "Cannot determine parent.");
        }
        return p;
    }

    public static string RemoveTrailingDirectorySeparator(this string path)
    {
        if (path.Length == 0)
        {
            return path;
        }
        var last = path[path.Length - 1];

        if (last == Path.DirectorySeparatorChar || last == Path.AltDirectorySeparatorChar)
        {
            return path.Substring(0, path.Length - 1);
        }
        else
        {
            return path;
        }
    }

    public static string? ParentOrNull(this string path)
    {
        var p = Path.GetDirectoryName(path.RemoveTrailingDirectorySeparator());
        if (String.IsNullOrEmpty(p))
        {
            p = Path.GetDirectoryName(path.Absolute());
        }
        return p;
    }
    /// <summary>
    /// Reads all text in a file. Returns null on error.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>text</returns>
    public static async Task<string?> ReadAllTextAsync(this string path)
    {
        try
        {
            using (var r = new StreamReader(path))
            {
                return await r.ReadToEndAsync();
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Write all text to a file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="text"></param>
    /// <returns>path</returns>
    public static async Task<string> WriteAllTextAsync(this string path, string text)
    {
        using (var r = new StreamWriter(path.EnsureParentDirectoryExists()))
        {
            await r.WriteAsync(text);
        }
        return path;
    }

    /// <summary>
    /// Writes text to the file `path`, but only if `path` does not already contain the desired text.
    /// </summary>
    /// Useful when writing config files to avoid to re-buildi everything when nothing in the config has actually changed.
    /// <param name="path"></param>
    /// <param name="text"></param>
    /// <returns>path</returns>
    public static async Task<string> WriteAllTextIfChangedAsync(this string path, string text)
    {
        var hasChanged = !path.IsFile() || !object.Equals(await path.ReadAllTextAsync(), text);

        if (hasChanged)
        {
            await path.WriteAllTextAsync(text);
        }

        return path;
    }

    /// <summary>
    /// Convenience wrapper for a single outputFile. See <![CDATA[ IsOutOfDate(this IEnumerable<string> outputFiles, IEnumerable<string> inputFiles) ]]>
    /// </summary>
    /// <param name="outputFile"></param>
    /// <param name="inputFiles"></param>
    /// <returns></returns>
    public static bool IsOutOfDate(this string outputFile, IEnumerable<string> inputFiles)
    {
        return new[] { outputFile }.IsOutOfDate(inputFiles);
    }

    /// <summary>
    /// Returns true if outputFiles cannot have been built from inputFiles.
    /// </summary>
    /// <param name="outputFiles"></param>
    /// <param name="inputFiles"></param>
    /// <returns>True if outputFiles cannot have been built from inputFiles, false otherwise</returns>
    public static bool IsOutOfDate(this IEnumerable<string> outputFiles, IEnumerable<string> inputFiles)
    {
        outputFiles = outputFiles.ToList();
        var outputModified = outputFiles.LastWriteTimeUtc();
        var inputModified = inputFiles.Except(outputFiles).LastWriteTimeUtc();
        var isOutOfDate = outputModified < inputModified;
        if (Logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
        {
            Logger.Debug(@"IsOutOfDate: {isOutOfDate}

Input files:
{@inputFiles}

Output files:
{@outputFiles}",
            isOutOfDate,
            inputFiles.Select(_ => new { Path = _, Changed = _.LastWriteTimeUtc() }),
            outputFiles.Select(_ => new { Path = _, Changed = _.LastWriteTimeUtc() }));
        }
        return isOutOfDate;
    }

    /// <summary>
    /// Returns true if outputFiles cannot have been built from inputFiles.
    /// </summary>
    /// <param name="outputFiles"></param>
    /// <param name="inputFiles"></param>
    /// <returns>True if outputFiles cannot have been built from inputFiles, false otherwise</returns>
    public static bool IsOutOfDate(this IEnumerable<FileSystemInfo> outputFiles, IEnumerable<FileSystemInfo> inputFiles)
    {
        outputFiles = outputFiles.ToList();
        var outputModified = outputFiles.LastWriteTimeUtc();
        var inputModified = inputFiles.Except(outputFiles).LastWriteTimeUtc();
        var isOutOfDate = outputModified < inputModified;
        if (Logger.IsEnabled(Serilog.Events.LogEventLevel.Information) && isOutOfDate)
        {
            Logger.Information(@"Output files 

{@outputFiles}

are out of date because input files

{@inputFiles}

are more recent.
",
            isOutOfDate,
            outputFiles.Select(_ => new { Path = _, Changed = _.LastWriteTimeUtc() }),
            inputFiles.Select(_ => new { Path = _, Changed = _.LastWriteTimeUtc() })
                .Where(_ => _.Changed > outputModified).Take(20)
            );
        }
        return isOutOfDate;
    }

    /// <summary>
    /// Last time something was written to paths
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static DateTime LastWriteTimeUtc(this IEnumerable<string> paths)
    {
        var files = paths.Select(_ => new { Path = _, LastWrite = _.LastWriteTimeUtc() })
            .ToList();

        var m = files.MaxElement(_ => _.LastWrite);

        if (m == null)
        {
            return DateTime.MinValue;
        }

        return m.LastWrite;
    }

    /// <summary>
    /// Last time something was written to path.
    /// </summary>
    /// Returns DateTime.MinValue if file is not found.
    /// <param name="path"></param>
    /// <returns></returns>
    public static DateTime LastWriteTimeUtc(this string path)
    {
        return path.IsFile()
            ? new FileInfo(path).LastWriteTimeUtc
            : DateTime.MinValue;

    }

    /// <summary>
    /// Last time something was written to paths
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static DateTime LastWriteTimeUtc(this IEnumerable<FileSystemInfo> paths)
    {
        var files = paths.Select(_ => new { Path = _, LastWrite = _.LastWriteTimeUtc() })
            .ToList();

        var m = files.MaxElement(_ => _.LastWrite);

        if (m == null)
        {
            return DateTime.MinValue;
        }

        return m.LastWrite;
    }

    /// <summary>
    /// Last time something was written to path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DateTime LastWriteTimeUtc(this FileSystemInfo path)
    {
        return path is FileInfo f
            ? f.LastWriteTimeUtc
            : DateTime.MinValue;
    }

    /// <summary>
    /// Returns true if path points to an existing file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFile(this string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Returns true if path points to an existing directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsDirectory(this string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// True, if path points to an existing file system element
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool Exists(this string path)
    {
        return path.IsFile() || path.IsDirectory();
    }

    /// <summary>
    /// Split path into directory parts
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string[] SplitDirectories(this string path)
    {
        if (String.IsNullOrEmpty(path))
        {
            return new string[] { };
        }

        path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return path.Split(Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Combine directory parts into path
    /// </summary>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string CombineDirectories(this IEnumerable<string> parts)
    {
        return Path.Combine(parts.ToArray());
    }

    /// <summary>
    /// Start a glob
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pattern">Glob pattern to include. If omitted, an empty glob is returned.</param>
    /// <returns></returns>
    public static Glob Glob(this string path, string? pattern = null)
    {
        var g = new Glob(path);
        if (pattern != null)
        {
            g = g.Include(pattern);
        }
        return g;
    }

    /// <summary>
    /// Gets a FileInfo or DirectoryInfo or null if file system object does not exist.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static FileSystemInfo? Info(this string path)
    {
        if (path.IsFile())
        {
            return new FileInfo(path);
        }
        else if (path.IsDirectory())
        {
            return new DirectoryInfo(path);
        }
        else
        {
            return null;
        }
    }

    static readonly Regex invalidPosixFileNameCharactersPattern = new Regex("[^0-9A-Za-z-._]");

    public static bool IsValidPosixFileName(this string x)
    {
        return (x.Length <= maxFileNameLength) && !invalidPosixFileNameCharactersPattern.IsMatch(x);
    }

    public static string MakeValidPosixFileName(this string x, int maxLength = 250)
    {
        x = invalidPosixFileNameCharactersPattern.Replace(x, "_");

        if (x.Length > maxLength)
        {
            var e = x.Extension();
            var maxBaseNameLength = maxLength - e.Length;
            if (maxBaseNameLength < 4)
            {
                x = x.TruncateMd5(maxLength);
            }
            else
            {
                x = x.FileNameWithoutExtension().TruncateMd5(maxBaseNameLength) + e;
            }
        }
        return x;
    }

    static readonly char[] invalidCharacters = Path.GetInvalidFileNameChars();
    const int maxFileNameLength = 255;
    static readonly Regex invalidFileNameCharactersPattern = new Regex("(" +
        invalidCharacters.Select(_ => Regex.Escape(new string(_, 1))).Join("|")
        + ")");

    /// <summary>
    /// true, if x is a valid file name 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static bool IsValidFileName(this string x)
    {
        return (x.IndexOfAny(invalidCharacters) < 0) && (x.Length <= maxFileNameLength);
    }

    public static string ToFileName(this DateTime time)
    {
        return time.ToString("o").MakeValidPosixFileName();
    }

    public static string ToShortFileName(this DateTime time)
    {
        return time.Ticks.BaseConvert(symbols09az);
    }

    readonly static char[] symbols09az = Enumerable.Range('0', '9' - '0' + 1).Concat(Enumerable.Range('A', 'Z' - 'A' + 1))
        .Select(_ => (char)_)
        .ToArray();

    /// <summary>
    /// Calculates a valid file name from x.
    /// </summary>
    /// * tries to use x unmodified
    /// * replace all invalid characters with _
    /// * shorten and replace tail with md5 checksum if too long.
    /// <param name="x"></param>
    /// <returns></returns>
    public static string MakeValidFileName(this string x)
    {
        if (x.IndexOfAny(invalidCharacters) >= 0)
        {
            x = invalidFileNameCharactersPattern.Replace(x, "_");
        }

        if (x.Length > maxFileNameLength)
        {
            var e = x.Extension();
            if (e.Length > maxFileNameLength)
            {
                x = x.TruncateMd5(maxFileNameLength);
            }
            else
            {
                x = x.FileNameWithoutExtension().TruncateMd5(maxFileNameLength - e.Length) + e;
            }
        }
        return x;
    }

    /// <summary>
    /// Copies a directory tree. 
    /// </summary>
    /// Post condition: all files found in the source tree are also found in dest
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <param name="useHardlinks">If possible, use hard links</param>
    /// <param name="overwrite">Overwrite existing files</param>
    /// <returns></returns>
    public static async Task<string> CopyTree(
        this string source,
        string dest,
        bool useHardlinks = false,
        bool overwrite = true)
    {
        var copyFile = useHardlinks
            ? new Func<FileInfo, string, Task>((s, d) => CopyHardlink(s, d))
            : new Func<FileInfo, string, Task>((s, d) => CopyFile(s, d, overwrite));

        try
        {
            return await DoCopyTree(source, dest, copyFile);
        }
        catch (Exception)
        {
            if (useHardlinks)
            {
                // retry without hard links
                return await CopyTree(source, dest, useHardlinks: false);
            }
            throw;
        }
    }

    static async Task<string> DoCopyTree(string source, string dest, Func<FileInfo, string, Task> copyFile)
    {
        if (source.IsFile())
        {
            await copyFile(new FileInfo(source), dest);
        }
        else if (source.IsDirectory())
        {
            dest.EnsureDirectoryExists();
            foreach (var i in source.EnumerateFileSystemEntries())
            {
                await DoCopyTree(i, dest.Combine(i.FileName()), copyFile);
            }
        }
        return dest;
    }

    static bool CanSkip(FileInfo source, FileInfo dest)
    {
        return source.Length == dest.Length
            && Math.Abs((source.LastWriteTimeUtc - dest.LastWriteTimeUtc).TotalSeconds) < 1.0;
    }

    static async Task<string> CopyFile(this string source, string dest, bool overwrite = false)
    {
        return await new FileInfo(source).CopyFile(dest, overwrite);
    }

    static async Task<string> CopyFile(this FileInfo source, string dest, bool overwrite)
    {
        // do we need to copy at all?
        if (dest.Info() is FileInfo fileInfo && CanSkip(source, fileInfo))
        {
            return dest;
        }

        // parent directory of dest could not exist. Retry once.
        try
        {
            File.Copy(source.FullName, dest, overwrite);
        }
        catch (DirectoryNotFoundException)
        {
            dest.EnsureParentDirectoryExists();
            File.Copy(source.FullName, dest, overwrite);
        }

        return await Task.FromResult(dest);
    }

    static async Task<string> CopyHardlink(FileInfo source, string dest)
    {
        // do we need to copy at all?
        if (dest.Info() is FileInfo destInfo && CanSkip(source, destInfo))
        {
            return dest;
        }

        // parent directory of dest could not exist. Retry once.
        try
        {
            source.FullName.CreateHardlink(dest);
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            dest.EnsureParentDirectoryExists();
            source.FullName.CreateHardlink(dest);
        }

        return await Task.FromResult(dest);
    }

    /// <summary>
    /// Changes the root directory of path from source to dest.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static string ChangeRoot(this string path, string source, string dest)
    {
        if (path.StartsWith(source))
        {
            return dest + path.Substring(source.Length);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(path), path, $"must start with {source}");
        }
    }

    static IFileSystem FileSystem { get; } = new Windows.FileSystem();

    /// <summary>
    /// Create a file system hard link
    /// </summary>
    /// <param name="path"></param>
    /// <param name="linkPath"></param>
    /// <returns></returns>
    public static string CreateHardlink(this string path, string linkPath)
    {
        FileSystem.CreateHardLink(linkPath, path);
        return linkPath;
    }

    /// <summary>
    /// Get information about a hard link
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IHardLinkInfo HardlinkInfo(this string path)
    {
        return FileSystem.GetHardLinkInfo(path);
    }

    /// <summary>
    /// Make sure that path does not exist, no matter if directory or file
    /// </summary>
    /// This method is to be used with caution. It can wipe whole directory trees.
    /// <param name="path">path to be deleted</param>
    /// <returns>path</returns>
    public static async Task<string> EnsureNotExists(this string path)
    {
        if (path.IsFile())
        {
            path.EnsureFileNotExists();
            await Task.CompletedTask;
        }
        else if (path.IsDirectory())
        {
            Logger.Information("Delete {directory}", path);
            foreach (var i in path.Glob("*"))
            {
                await i.EnsureNotExists();
            }
            Directory.Delete(path);
        }
        return path;
    }

    /// <summary>
    /// Gets the ProgramData directory for type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetProgramDataDirectory(this System.Type type)
    {
        var assembly = type.Assembly;
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Combine(new[]
        {
                assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                type.Name
            }.Where(_ => !String.IsNullOrEmpty(_))
        .NotNull()
        .ToArray());
    }

    /// <summary>
    /// Gets the ProgramData directory for type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetProgramDataDirectory(this Assembly assembly)
    {
        var p = new[]
        {
                assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                assembly.GetName().Name
            }
        .NotNull()
        .Select(_ => _.MakeValidFileName());

        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Combine(p.ToArray());
    }

    /// <summary>
    /// Enumerate the file system entries in dir
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateFileSystemEntries(this string dir)
    {
        return System.IO.Directory.EnumerateFileSystemEntries(dir);
    }

    /// <summary>
    /// Enumerate the directories in dir
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateDirectories(this string dir)
        => Directory.EnumerateDirectories(dir);

    /// <summary>
    /// Enumerate the directories in dir
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateDirectories(this string path, string searchPattern)
        => Directory.EnumerateDirectories(path, searchPattern);

    /// <summary>
    /// Enumerate the files in dir
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateFiles(this string dir)
        => Directory.EnumerateFiles(dir);

    /// <summary>
    /// Enumerate the files in dir
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static IEnumerable<string> EnumerateFiles(this string dir, string searchPattern)
        => Directory.EnumerateFiles(dir, searchPattern);

    /// <summary>
    /// Move path to the Windows Recycle Bin
    /// </summary>
    /// <param name="path"></param>
    public static void MoveToRecycleBin(this string path)
    {
        FileOperationApiWrapper.MoveToRecycleBin(path);
    }

    /// <summary>
    /// Changes the file name of path to name
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string WithName(this string path, string name)
    {
        return path.Parent().Combine(name);
    }

    /// <summary>
    /// Changes the extension of path to extension
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string WithExtension(this string path, string extension)
    {
        return path.WithName(path.FileNameWithoutExtension() + extension);
    }

    /// <summary>
    /// Adds filenamePostfix to the file name of path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filenamePostfix"></param>
    /// <returns></returns>
    public static string CatName(this string path, string filenamePostfix)
    {
        return path + filenamePostfix;
    }

    /// <summary>
    /// Moves file or directory from to to
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static Task<string> Move(this string from, string to) => Task.Factory.StartNew(() =>
    {
        if (from.IsFile())
        {
            System.IO.File.Move(from, to);
            return to;
        }
        else if (from.IsDirectory())

        {
            System.IO.Directory.Move(from, to);
            return to;
        }
        else
        {
            throw new FileNotFoundException("cannot move", from);
        }
    });

    /// <summary>
    /// Reads 2 files to the end and compares for equality
    /// </summary>
    /// <param name="path0"></param>
    /// <param name="path1"></param>
    /// <returns></returns>
    public static async Task<bool> IsContentEqual(this string path0, string path1)
    {
        using (var s0 = File.OpenRead(path0))
        using (var s1 = File.OpenRead(path1))
        {
            return await s0.IsContentEqual(s1);
        }
    }

    /// <summary>
    /// Reads 2 streams to the end and compares for equality
    /// </summary>
    /// <param name="stream0"></param>
    /// <param name="stream1"></param>
    /// <returns></returns>
    public static async Task<bool> IsContentEqual(this Stream stream0, Stream stream1)
    {
        const int bufferSize = 4096;
        var buffer0 = new byte[bufferSize];
        var buffer1 = new byte[bufferSize];

        while (true)
        {
            var read0 = await stream0.ReadAsync(buffer0, 0, buffer0.Length);
            var read1 = await stream1.ReadAsync(buffer1, 0, buffer1.Length);
            if (read0 <= 0 && read0 == read1)
            {
                return true;
            }
            if (!buffer0.Take(read0).SequenceEqual(buffer1.Take(read1)))
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Returns a path in the same directory as p that does not yet exist
    /// </summary>
    /// Does this by adding numbers (.0, .1, ...) to p.
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetNotExisting(this string path)
    {
        if (!path.Exists())
        {
            return path;
        }

        for (var i = 0; ; ++i)
        {
            var backup = path + $".{i}";
            if (!backup.Exists())
            {
                return backup;
            }
        }
    }

    public static string RelativeTo(this string path, string relativeTo)
    {
        var p = path.SplitDirectories();
        var r = relativeTo.SplitDirectories();
        if (p.StartsWith(r))
        {
            return Path.Combine(p.Skip(r.Count()).ToArray());
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(path), path, $"must be a child of {relativeTo}");
        }
    }

    public static Task<string> Touch(this string path) => Task.Factory.StartNew(() =>
    {
        using (var myFileStream = File.Open(path.EnsureParentDirectoryExists(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            myFileStream.Close();
        }
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
        return path;
    });

    /// <summary>
    /// Move path to a numbered backup location if it exists.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<string?> MoveToBackup(this string path)
    {
        if (!path.Exists())
        {
            return null;
        }

        for (var i = 0; ; ++i)
        {
            var backup = path + $".backup{i}";
            if (!backup.Exists())
            {
                Logger.Information("Creating backup of {path} at {backup}.", path, backup);
                return await path.Move(backup);
            }
        }
    }

    public static IBackup Backup(this string sourceRoot)
    {
        return sourceRoot.Backup(Path.GetTempPath());
    }

    public static IBackup Backup(this string sourceRoot, string dest)
    {
        return new BackupDirectory(sourceRoot, dest);
    }

    /// <summary>
    /// True, if path is identical to parent or a descendant of parent
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public static bool IsDescendantOrSelf(this string path, string parent)
    {
        return parent.SplitDirectories()
            .ZipOr(path.SplitDirectories(), (a, b) => FileNameEqual(a, b), a => false)
            .All(_ => _);
    }

    static bool FileNameEqual(string? a, string? b)
    {
        if (a == null || b == null)
        {
            return b == null && a == null;
        }
        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// inputFile => partFile.000, partFile.001, partFile.002, ...
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="partFile"></param>
    /// <param name="partLength"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<string>> SplitFile(this string inputFile, string partFile, long partLength)
    {
        var info = new FileInfo(inputFile);
        if (info.Length <= partLength)
        {
            await inputFile.CopyTree(partFile);
            return new[] { partFile };
        }
        else
        {
            var buffer = new byte[0x10000];
            var parts = new List<string>();

            using (var r = File.OpenRead(inputFile))
            {
                bool readEnd = false;
                for (int partNumber = 0; !readEnd; ++partNumber)
                {
                    var currentPartFile = partFile + partNumber.ToString("D3");
                    using (var w = File.OpenWrite(currentPartFile))
                    {
                        long bytesWritten = 0;
                        while (bytesWritten < partLength && !readEnd)
                        {
                            var bytesToRead = (int)Math.Min((long)buffer.Length, (partLength - bytesWritten));
                            var bytesRead = await r.ReadAsync(buffer, 0, bytesToRead);
                            if (bytesRead <= 0)
                            {
                                readEnd = true;
                            }
                            else
                            {
                                await w.WriteAsync(buffer, 0, bytesRead);
                                bytesWritten += bytesRead;
                            }
                        }
                        if (bytesWritten > 0)
                        {
                            parts.Add(currentPartFile);
                        }
                    }
                }
            }
            return parts;
        }
    }
}
