using Amg.FileSystem;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amg.Extensions;

public static class Json
{
    /// <summary>
    /// Deserialize JSON directly from a file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<T?> Read<T>(string path)
    {
        using var file = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(file);
    }

    /// <summary>
    /// Serialize JSON directly to a file.
    /// </summary>
    public static async Task Write<T>(string path, T data)
    {
        using var file = File.Create(path.EnsureParentDirectoryExists());
        await JsonSerializer.SerializeAsync(file, data);
    }

    public static string Hash<T>(T data)
    {
        var json = JsonSerializer.Serialize(data);
        return json.Md5Checksum();
    }
}
