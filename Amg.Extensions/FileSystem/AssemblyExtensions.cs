using Amg.Extensions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Amg.FileSystem;

public static class AssemblyExtensions
{
    public static string Directory(this Assembly assembly) => assembly.Location.Parent();

    public static string Path(this Assembly assembly)
    {
        var p = new[]
        {
                assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                assembly.GetName().Name
            }
        .NotNull()
        .Select(_ => _.MakeValidFileName());
        return PathExtensions.CombineDirectories(p);
    }

    /// <summary>
    /// Gets the ProgramData directory for assembly
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetProgramDataDirectory(this Assembly assembly) => assembly.CommonApplicationData();

    /// <summary>
    /// Gets the ProgramData directory for assembly
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string CommonApplicationData(this Assembly assembly)
        => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).Combine(assembly.Path());

    public static string LocalApplicationData(this Assembly assembly)
        => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Combine(assembly.Path());

    public static string TempPath(this Assembly assembly)
        => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Combine(assembly.Path());
}
