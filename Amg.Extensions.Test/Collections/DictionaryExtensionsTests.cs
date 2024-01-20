using System.Collections.Specialized;

namespace Amg.Collections;

[TestFixture]
public class DictionaryExtensionsTests
{
    [Test]
    public void GetOrAdd()
    {
        const int count = 100;
        var d = Enumerable.Range(0, count).ToDictionary(x => x, x => x.ToString());
        Assert.That(d.GetOrAdd(0, () => "0"), Is.EqualTo("0"));
        Assert.That(d.Count, Is.EqualTo(count));
        Assert.That(d.GetOrAdd(count, () => count.ToString()), Is.EqualTo(count.ToString()));
        Assert.That(d.Count, Is.EqualTo(count + 1));
    }

    [Test]
    public void Merge()
    {
        const int count = 100;
        var d0 = Enumerable.Range(0, count).ToDictionary(x => x, x => x.ToString());
        var d1 = Enumerable.Range(count / 2, count).ToDictionary(x => x, x => x.ToString());
        var m = d0.Merge(d1);
        Assert.That(m.Count, Is.EqualTo(count / 2 * 3));
    }

    [Test]
    public void Add()
    {
        const int count = 100;
        var sd = new StringDictionary();
        var d = Enumerable.Range(0, count).ToDictionary(x => x.ToString(), x => x.ToString());
        sd.Add(d);
        Assert.That(sd.Count, Is.EqualTo(count));
        sd.Add(d);
        Assert.That(sd.Count, Is.EqualTo(count));
    }
}
