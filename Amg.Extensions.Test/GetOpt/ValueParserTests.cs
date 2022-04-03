using System.Linq;

namespace Amg.GetOpt.Test;

[TestFixture]
class ValueParserTests
{
    [Test]
    public void ParseBool()
    {
        var args = new ParserState(new[] { "true" });
        var p = new ValueParser();
        Assert.That((bool)p.Parse(args, typeof(bool))!);
    }

    [Test]
    public void ParseStringArray()
    {
        var data = new[] { "a", "b", "c" };
        var args = new ParserState(data);
        var p = new ValueParser();
        var parsed = (string[])p.Parse(args, typeof(string[]))!;
        Assert.That(parsed.SequenceEqual(data));
    }
}
