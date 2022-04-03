namespace Amg.Extensions;

[TestFixture]
public class TimeIntervalTests
{
    [Test]
    public void WorkWithTimeInterval()
    {
        var from = new DateTime(2022, 4, 3, 12, 34, 23);
        var to = new DateTime(2024, 9, 2, 9, 57, 32);
        var i = new TimeInterval(from, to);
        Assert.That(i.From, Is.EqualTo(from));
        Assert.That(i.To, Is.EqualTo(to));
        Assert.That(i.Duration, Is.EqualTo(to-from));

        Assert.That(TimeInterval.MaxValue.Contains(i));

        Assert.That(TimeInterval.Month(from), Is.EqualTo(
            new TimeInterval(new DateTime(2022, 4, 1), new DateTime(2022, 5, 1))));

        Assert.That(TimeInterval.Month(from).NextMonth(), Is.EqualTo(
            new TimeInterval(new DateTime(2022, 5, 1), new DateTime(2022, 6, 1))));

        Assert.That(TimeInterval.Week(from), Is.EqualTo(
            TimeInterval.Parse("[2022-03-28, 2022-04-04[")));

        Assert.That(TimeInterval.Week(from).NextWeek(), Is.EqualTo(
            TimeInterval.Parse("[2022-04-04, 2022-04-11[")));

        // round-trip parse
        Assert.That(TimeInterval.Parse(i.ToString()), Is.EqualTo(i));

        var afterIInterval = new TimeInterval(to, to.AddDays(1));
        Assert.That(i.Intersect(afterIInterval).Duration, Is.EqualTo(TimeSpan.Zero));

        var beforeI = i.From.AddMinutes(-1);
        Assert.That(i.Limit(beforeI), Is.EqualTo(i.From));
        var afterI = afterIInterval.To;
        Assert.That(i.Limit(afterI), Is.EqualTo(i.To));
        var inI = i.From.AddMinutes(1);
        Assert.That(i.Limit(inI), Is.EqualTo(inI));

        Assert.That(i.Contains(beforeI), Is.False);
        Assert.That(i.Contains(afterI), Is.False);
        Assert.That(i.Contains(inI), Is.True);
        Assert.That(i.Offset(_ => _.AddMinutes(1)), Is.EqualTo(
            new TimeInterval(
                new DateTime(2022, 4, 3, 12, 35, 23),
                new DateTime(2024, 9, 2, 9, 58, 32))));

        Assert.That(TimeInterval.FiscalYear(i.From), Is.EqualTo(
            new TimeInterval(new DateTime(2021, 10, 1), new DateTime(2022, 10, 1))));

        Assert.That(TimeInterval.Year(i.From), Is.EqualTo(
            new TimeInterval(new DateTime(2022, 1, 1), new DateTime(2023, 1, 1))));

        Assert.That(i.CompareTo(i), Is.EqualTo(0));
        Assert.That(i.CompareTo(i.Offset(_ => _.AddMonths(-1))), Is.EqualTo(1));
        Assert.That(i.CompareTo(i.Offset(_ => _.AddMonths(+1))), Is.EqualTo(-1));
        Assert.That(i.CompareTo(new TimeInterval(i.From, i.To.AddMonths(1))), Is.EqualTo(-1));

        Assert.That(i.Equals(i), Is.True);

        Assert.That(i.GetHashCode(), Is.EqualTo(1083696366));

        Assert.That(i == afterIInterval, Is.False);
        Assert.That(i != afterIInterval, Is.True);
        Assert.That(i < afterIInterval, Is.True);
        Assert.That(i <= afterIInterval, Is.True);
        Assert.That(i > afterIInterval, Is.False);
        Assert.That(i >= afterIInterval, Is.False);
    }
}
