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
        type.GetProgramDataDirectory().IsValidPath().Should().BeTrue();
        type.TempPath().IsValidPath().Should().BeTrue();
        type.LocalApplicationData().IsValidPath().Should().BeTrue();
    }
}
