namespace Amg.Extensions;

[TestFixture]
public class TimeIntervalTests
{
    [Test]
    public void WorkWithTimeInterval()
    {
        var from = new DateTime(2022, 4, 3, 12, 34, 23, DateTimeKind.Utc);
        var to = new DateTime(2024, 9, 2, 9, 57, 32, DateTimeKind.Utc);
        var i = new TimeInterval(from, to);
        Assert.That(i.From, Is.EqualTo(from));
        Assert.That(i.To, Is.EqualTo(to));
        Assert.That(i.Duration, Is.EqualTo(to-from));

        TimeInterval.MaxValue.Contains(i).Should().BeTrue();

        // months
        TimeInterval.Month(from).Should().Be(new TimeInterval(new DateTime(2022, 4, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 5, 1, 0, 0, 0, DateTimeKind.Utc)));
        TimeInterval.Month(from).NextMonth().Should().Be(new TimeInterval(new DateTime(2022, 5, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc)));

        // weeks
        TimeInterval.Week(from).Should().Be(TimeInterval.Parse("[2022-03-28, 2022-04-04["));
        TimeInterval.Week(from).NextWeek().Should().Be(TimeInterval.Parse("[2022-04-04, 2022-04-11["));

        // round-trip parse
        TimeInterval.Parse(i.ToString()).Should().Be(i);

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
                new DateTime(2022, 4, 3, 12, 35, 23, DateTimeKind.Utc),
                new DateTime(2024, 9, 2, 9, 58, 32, DateTimeKind.Utc))));

        Assert.That(TimeInterval.FiscalYear(i.From), Is.EqualTo(
            new TimeInterval(new DateTime(2021, 10, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2022, 10, 1, 0, 0, 0, DateTimeKind.Utc))));

        Assert.That(TimeInterval.Year(i.From), Is.EqualTo(
            new TimeInterval(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc))));

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
