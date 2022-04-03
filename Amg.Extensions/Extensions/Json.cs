using Amg.FileSystem;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amg.Extensions;

public static class Json
{
    public static async Task<T?> Read<T>(string path)
    {
        // deserialize JSON directly from a file
        using var file = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(file);
    }

    /// <summary>
    /// serialize JSON directly to a file
    /// </summary>
    public static async Task Write<T>(string path, T data)
    {
        using var file = File.OpenWrite(path.EnsureParentDirectoryExists());
        await JsonSerializer.SerializeAsync(file, data);
    }

    public static string Hash<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return json.Md5Checksum();
    }
}
