using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amg.FileSystem.Unix;

internal class FileSystem : IFileSystem
{
    public FileSystem()
    {
        PathEqualityComparer = StringComparer.Ordinal;
    }

    public Task CopyFile(string existingFileName, string newFileName, IProgress<CopyFileProgress>? progress = null, CancellationToken cancellationToken = default, CopyFileOptions? options = null) => Task.Factory.StartNew(() =>
    {
        options ??= new CopyFileOptions();
        System.IO.File.Copy(existingFileName, newFileName, options.Overwrite);
    });

    public Task CreateHardLink(string fileName, string existingFileName)
    {
        throw new NotImplementedException();
    }

    public Task<IHardLinkInfo> GetHardLinkInfo(string path)
    {
        throw new NotImplementedException();
    }

    public IEqualityComparer<string> PathEqualityComparer { get; init; }
}
