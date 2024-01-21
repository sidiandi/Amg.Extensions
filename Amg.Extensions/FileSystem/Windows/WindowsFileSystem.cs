using Amg.FileSystem.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Amg.FileSystem.Windows
{
    /// <summary>
    /// Operations that access the Windows file system
    /// </summary>
    internal class FileSystem : IFileSystem
    {
        /// <summary>
        /// Create a hardlink at fileName that points to existingFileName
        /// </summary>
        /// <param name="fileName">Path of the newly created link</param>
        /// <param name="existingFileName">Path of the existing file to which the link will point</param>
        public Task CreateHardLink(string fileName, string existingFileName) => Task.Factory.StartNew(() =>
        {
            NativeMethods.CreateHardLink(fileName, existingFileName, IntPtr.Zero)
                .CheckApiCall($"Cannot create hard link: {fileName} -> {existingFileName}");
        });

        public async Task<IHardLinkInfo> GetHardLinkInfo(string path)
        {
            return await HardLinkInfo.Get(path);
        }

#pragma warning disable S1144 // Unused private types or members should be removed
        const int ERROR_ALREADY_EXISTS = 183;
        const int ERROR_PATH_NOT_FOUND = 3;
#pragma warning restore S1144 // Unused private types or members should be removed

        public Task CopyFile(
            string existingFileName,
            string newFileName,
            IProgress<CopyFileProgress>? progress = null,
            CancellationToken cancellationToken = new CancellationToken(),
            CopyFileOptions? options = null) => Task.Factory.StartNew(() =>
        {
            Int32 pbCancel = 0;

            if (options == null)
            {
                options = new CopyFileOptions();
            }

            var begin = DateTime.UtcNow;

            var progressCallback = progress != null
                ? new NativeMethods.CopyProgressRoutine(
                (
                    long TotalFileSize,
                    long TotalBytesTransferred,
                    long StreamSize,
                    long StreamBytesTransferred,
                    uint dwStreamNumber,
                    NativeMethods.CopyProgressCallbackReason dwCallbackReason,
                    IntPtr hSourceFile,
                    IntPtr hDestinationFile,
                    IntPtr lpData
                ) =>
                {
                    progress.Report(new CopyFileProgress(
                        begin,
                        TotalFileSize,
                        TotalBytesTransferred,
                        StreamSize,
                        StreamBytesTransferred,
                        (int)dwStreamNumber));

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return NativeMethods.CopyProgressResult.PROGRESS_CANCEL;
                    }
                    else
                    {
                        return NativeMethods.CopyProgressResult.PROGRESS_CONTINUE;
                    }
                })
                : null;

            NativeMethods.CopyFileEx(
                existingFileName,
                newFileName,
                progressCallback,
                IntPtr.Zero,
                ref pbCancel,
                (options.AllowDecryptedDestination ? NativeMethods.CopyFileFlags.COPY_FILE_ALLOW_DECRYPTED_DESTINATION : 0) |
                (options.CopySymlink ? NativeMethods.CopyFileFlags.COPY_FILE_COPY_SYMLINK : 0) |
                (options.FailIfExists ? NativeMethods.CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS : 0) |
                (options.NoBuffering ? NativeMethods.CopyFileFlags.COPY_FILE_NO_BUFFERING : 0) |
                (options.OpenSourceForWrite ? NativeMethods.CopyFileFlags.COPY_FILE_OPEN_SOURCE_FOR_WRITE : 0) |
                (options.Restartable ? NativeMethods.CopyFileFlags.COPY_FILE_RESTARTABLE : 0))
                .CheckApiCall(String.Format("copy from {0} to {1}", existingFileName, newFileName));
        }, TaskCreationOptions.LongRunning);

        public IEqualityComparer<string> PathEqualityComparer { get; init; } = StringComparer.OrdinalIgnoreCase;
    }
}
