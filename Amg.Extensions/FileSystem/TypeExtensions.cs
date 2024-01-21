namespace Amg.FileSystem;

public static class TypeExtensions
{
    public static string Path(this Type type) => type.Assembly.Path().Combine(type.FullName!.MakeValidFileName());

    /// <summary>
    /// Gets the ProgramData directory for type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetProgramDataDirectory(this System.Type type) => type.CommonApplicationData();

    public static string CommonApplicationData(this System.Type type)
        => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Combine(type.Path());

    public static string LocalApplicationData(this System.Type type)
        => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Combine(type.Path());

    public static string TempPath(this System.Type type)
        => System.IO.Path.GetTempPath().Combine(type.Path());

}
