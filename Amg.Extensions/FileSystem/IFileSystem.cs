using System.Threading;
using System.Threading.Tasks;

namespace Amg.FileSystem;

public interface IFileSystem
{
    /// <summary>
    /// Copy a file.
    /// </summary>
    /// <param name="existingFileName">Name of the copy source file.</param>
    /// <param name="newFileName">Name of the copy destination file.</param>
    /// <param name="progress">To report progress information back to the caller. Can be null.</param>
    /// <param name="cancellationToken">To cancel the copy operation. When cancelled, the destination file will not exist.</param>
    /// <param name="options">Options for the copy operation.</param>
    Task CopyFile(
        string existingFileName,
        string newFileName,
        IProgress<CopyFileProgress>? progress = null,
        System.Threading.CancellationToken cancellationToken = new CancellationToken(),
        CopyFileOptions? options = null);

    /// <summary>
    /// Returns information about a hard linked file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<IHardLinkInfo> GetHardLinkInfo(string path);

    /// <summary>
    /// Create a new hard link.
    /// </summary>
    /// Precondition: (existingFileName.IsFile &amp;&amp; !fileName.IsFile &amp;&amp; fileName.Parent.IsDirectory)
    /// Postcondition: (fileName.IsFile)
    /// Will throw an exception ? when fileName does not have the same root as existingFileName
    /// <param name="fileName">Path where the new hard link shall be created.</param>
    /// <param name="existingFileName">Existing file for which a hard link will be created.</param>
    Task CreateHardLink(string fileName, string existingFileName);

    IEqualityComparer<string> PathEqualityComparer { get; init; }
}

public static class FileSystem
{
    static IFileSystem? _current = null;

    public static IFileSystem Current
    {
        get
        {
            if (_current is null)
            {
                _current = System.Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => new Windows.FileSystem(),
                    PlatformID.Unix => new Unix.FileSystem(),
                    _ => throw new NotSupportedException()
                };
            }
            return _current;
        }
    }
}