using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Amg;

public sealed record TimeInterval : IComparable<TimeInterval>, IEquatable<TimeInterval>
{
    readonly DateTime from;
    readonly DateTime to;

    [JsonConstructor]
    public TimeInterval(DateTime from, DateTime to)
    {
        this.from = from;
        this.to = to;
    }

    public TimeInterval(DateTime from, Func<DateTime, DateTime> to)
        : this(from, to(from))
    {
    }

    public TimeInterval(Func<DateTime, DateTime> from, DateTime to)
        : this(from(to), to)
    {
    }

    public static TimeInterval MaxValue => new TimeInterval(DateTime.MinValue, DateTime.MaxValue);


    public DateTime From => from;
    public DateTime To => to;
    public TimeSpan Duration => To - From;

    public override string ToString() => $"[{From:o}, {To:o}[";
    public static TimeInterval Parse(string timeIntervalString)
    {
        var oDateTime = @"\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}:\d{2}\.\d+Z?)?";
        var re = $@"\[(?<from>{oDateTime}),\s*(?<to>{oDateTime})\[";
        var m = Regex.Match(timeIntervalString, re);
        if (m.Success)
        {
            return new TimeInterval(
                DateTime.Parse(m.Groups["from"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                DateTime.Parse(m.Groups["to"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(timeIntervalString), timeIntervalString, $"Must be {re}");
        }
    }

    /// <summary>
    /// US Fiscal Year, begins on October 1st, see https://en.wikipedia.org/wiki/Fiscal_year#United_States
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static TimeInterval FiscalYear(DateTime time)
    {
        var f = time.Month < 10
            ? Date(time, time.Year - 1, 10, 1)
            : Date(time, time.Year, 10, 1);
        return new TimeInterval(f, f.AddYears(1));
    }

    public static TimeInterval Year(DateTime time)
    {
        var f = Date(time, time.Year, 1, 1);
        return new TimeInterval(f, f.AddYears(1));
    }

    static DateTime Date(DateTime time, int year, int month, int day) => new DateTime(year, month, day, 0, 0, 0, time.Kind);

    public static TimeInterval Month(DateTime time)
    {
        var f = Date(time, time.Year, time.Month, 1);
        return new TimeInterval(f, f.AddMonths(1));
    }

    public TimeInterval NextMonth() => Offset(_ => _.AddMonths(1));

    /// <summary>
    /// The week time interval which contains time.
    /// </summary>
    /// According to ISO 8601, the week starts on Monday.
    /// <param name="time"></param>
    /// <returns></returns>
    public static TimeInterval Week(DateTime time)
    {
        var d = (int)time.DayOfWeek - 1;
        if (d < 0) d += 7;
        return new TimeInterval(time.Date.AddDays(-d), _ => _.AddDays(7));
    }

    public TimeInterval NextWeek() => Offset(_ => _.AddDays(7));

    public TimeInterval Intersect(TimeInterval right) => new TimeInterval(Limit(right.From), Limit(right.To));
    public DateTime Limit(DateTime t)
    {
        if (t < From) return From;
        if (t > To) return To;
        return t;
    }
    public bool Contains(DateTime t) => From <= t && t < To;
    public bool Contains(TimeInterval t) => Contains(t.From) && From <= t.To && t.To <= To;

    public TimeInterval Offset(Func<DateTime, DateTime> offset) => new TimeInterval(offset(From), offset(To));

    public int CompareTo(TimeInterval? other)
    {
        if (other is null) return 1;
        var c = From.CompareTo(other.From);
        if (c != 0) return c;
        return To.CompareTo(other.To);
    }

    public bool Equals(TimeInterval? other)
    {
        return !(other is null) && (From.Equals(other.From) && To.Equals(other.To));
    }

    public override int GetHashCode()
    {
        return From.GetHashCode();
    }

    public static bool operator <(TimeInterval left, TimeInterval right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(TimeInterval left, TimeInterval right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(TimeInterval left, TimeInterval right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(TimeInterval left, TimeInterval right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
