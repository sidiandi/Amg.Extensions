using Amg.FileSystem.Core;

namespace Amg.FileSystem;

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