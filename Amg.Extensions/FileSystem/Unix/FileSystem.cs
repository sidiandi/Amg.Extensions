using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Amg.FileSystem.Unix;

internal class FileSystem : IFileSystem
{
    internal static class NativeMethods
    {
        [DllImport("libc")]
        public static extern int link(string oldpath, string newpath);
    }

    public FileSystem()
    {
        PathEqualityComparer = StringComparer.Ordinal;
    }

    public Task CopyFile(string existingFileName, string newFileName, IProgress<CopyFileProgress>? progress = null, CancellationToken cancellationToken = default, CopyFileOptions? options = null) => Task.Factory.StartNew(() =>
    {
        options ??= new CopyFileOptions();
        System.IO.File.Copy(existingFileName, newFileName, options.Overwrite);
    });

    public Task CreateHardLink(string fileName, string existingFileName) => Task.Factory.StartNew(() =>
    {
        var r = NativeMethods.link(existingFileName, fileName);
        if (r != 0)
        {
            throw new System.IO.IOException($"link({existingFileName}, {fileName}): {r}");
        }
    });

    public Task<IHardLinkInfo> GetHardLinkInfo(string path)
    {
        throw new NotImplementedException();
    }

    public IEqualityComparer<string> PathEqualityComparer { get; init; }
}
