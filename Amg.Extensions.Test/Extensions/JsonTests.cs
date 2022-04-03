using Amg.Collections;
using Amg.FileSystem;

namespace Amg.Extensions;

[TestFixture]
public class JsonTests
{
    [Test]
    public async Task WriteRead()
    {
        var dir = CreateTestDirectory();
        var file = dir.Combine("data.json");
        var r = PersonData.Sample();
        await Json.Write(file, r);
        var r1 = await Json.Read<PersonData>(file);
        Assert.That(r1, Is.EqualTo(r));
    }
}
