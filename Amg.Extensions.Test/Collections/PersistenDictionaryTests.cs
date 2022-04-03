using Amg.FileSystem;
using System.Collections;
using System.Collections.Generic;

namespace Amg.Collections;

[TestFixture]
public class PersistenDictionaryTests
{
    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1199:Nested code blocks should not be used", Justification = "<Pending>")]
    public void WorkWithPersistentDictionary()
    {
        int count = 100;
        var dir = CreateTestDirectory();
        var file = dir.Combine("dictionary");

        {
            using var d = new PersistentDictionary<int, string>(file);
            foreach (var i in Enumerable.Range(0, count))
            {
                d[i] = i.ToString();
            }
            Assert.That(d.ContainsKey(0), Is.True);
            Assert.That(d.ContainsKey(count), Is.False);

            Assert.That(d.ToArray().Count, Is.EqualTo(count));
            var counter = 0;
            foreach (var i in d)
            {
                counter++;
            }
            Assert.That(counter, Is.EqualTo(count));
            counter = 0;
            foreach (var i in ((IEnumerable)d))
            {
                counter++;
            }
            Assert.That(counter, Is.EqualTo(count));

            Assert.That(d.TryGetValue(42, out var value));
            Assert.That(value, Is.EqualTo("42"));

            d.Add(101, "hello");
            ++count;

            d.Add(new KeyValuePair<int, string>(102, "world"));
            ++count;
        }

        {
            using var d = new PersistentDictionary<int, string>(file);
            Assert.That(d.IsReadOnly, Is.False);

            var a = new KeyValuePair<int, string>[count];
            d.CopyTo(a, 0);
            Assert.That(a[99], Is.EqualTo(new KeyValuePair<int, string>(99, "99")));

            Assert.That(d.Contains(new KeyValuePair<int, string>(101, "hello")));
            Assert.That(d[101], Is.EqualTo("hello"));
            Assert.That(d[102], Is.EqualTo("world"));
            Assert.That(d.Count, Is.EqualTo(count));
            Assert.That(d[0], Is.EqualTo("0"));
            d.Remove(0);
            --count;
            Assert.That(d.TryGetValue(0, out var v), Is.False);

            Assert.That(d.Count, Is.EqualTo(count));
            Assert.That(d.Keys.Count, Is.EqualTo(count));
            Assert.That(d.Values.Count, Is.EqualTo(count));

            d[0] = "hello";
            ++count;
            Assert.That(d[0], Is.EqualTo("hello"));

            var removed = d.Remove(new KeyValuePair<int, string>(0, "hello"));
            Assert.That(removed);
            --count;
            Assert.That(d.Count, Is.EqualTo(count));

            removed = d.Remove(new KeyValuePair<int, string>(1, "hello"));
            Assert.That(removed, Is.False);

            d.Clear();
            Assert.That(d.Count, Is.EqualTo(0));
        }
    }

    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1199:Nested code blocks should not be used", Justification = "<Pending>")]
    public void StoreRecords()
    {
        var r = new PersonData(
            Name: "Alice",
            DateOfBirth: new DateTime(2022, 4, 1),
            Id: 123,
            Registered: true,
            new TimeInterval(new DateTime(2028, 9, 1), _ => _.AddYears(13))
        );

        var dir = CreateTestDirectory();
        var file = dir.Combine("data");

        {
            using var d = new PersistentDictionary<string, PersonData>(file, "Persons");
            d[r.Name] = r;
        }

        {
            using var d = new PersistentDictionary<string, PersonData>(file, "Persons");
            Assert.That(d.ContainsKey(r.Name));
            var r1 = d[r.Name];
            Assert.That(r1, Is.EqualTo(r));
        }
    }
}

public record PersonData(
    string Name,
    DateTime DateOfBirth,
    int Id,
    bool Registered,
    TimeInterval School)
{
    public static PersonData Sample() => new PersonData(
        Name: "Alice",
        DateOfBirth: new DateTime(2022, 4, 1),
        Id: 123,
        Registered: true,
        new TimeInterval(new DateTime(2028, 9, 1), _ => _.AddYears(13))
    );
}

