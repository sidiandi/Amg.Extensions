using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amg.FileSystem;

public record TestData(string Name);

public class AssemblyExtensionsTests
{
    [Test]
    public void GetDirectories()
    {
        var type = typeof(TestData);
        Console.WriteLine(type.GetProgramDataDirectory());
        Console.WriteLine(type.TempPath());
        Console.WriteLine(type.LocalApplicationData());
        Assert.Pass();
    }
}
