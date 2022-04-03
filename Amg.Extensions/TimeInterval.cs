using System;

namespace Amg;

public sealed class TimeInterval : IComparable<TimeInterval>, IEquatable<TimeInterval>
{
    public TimeInterval()
    {
    }

    public TimeInterval(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    public TimeInterval(DateTime from, Func<DateTime, DateTime> to)
        : this(from, to(from))
    {
    }

    public static TimeInterval MaxValue => new TimeInterval(DateTime.MinValue, DateTime.MaxValue);


    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public TimeSpan Duration => To - From;
    public static TimeInterval Month(DateTime time)
    {
        var from = new DateTime(time.Year, time.Month, 1);
        return new TimeInterval(from, from.AddMonths(1));
    }
    public TimeInterval NextMonth() => Offset(_ => _.AddMonths(1));


    public static TimeInterval Week(DateTime time)
    {
        var d = (int)time.DayOfWeek;
        return new TimeInterval(time.Date.AddDays(-d), _ => _.AddDays(7));
    }

    public TimeInterval NextWeek() => Offset(_ => _.AddDays(7));

    public TimeInterval Intersect(TimeInterval right) => new TimeInterval(Limit(right.From), Limit(right.To));
    DateTime Limit(DateTime t)
    {
        if (t < From) return From;
        if (t > To) return To;
        return t;
    }
    public bool Contains(DateTime t) => From <= t && t < To;
    public bool Contains(TimeInterval t) => Contains(t.From) && From <= t.To && t.To <= To;
    public override string ToString() => $"[{From:yyyy-MM-dd}, {To:yyy-MM-dd}[";

    public TimeInterval Offset(Func<DateTime, DateTime> offset) => new TimeInterval(offset(From), offset(To));

    public int CompareTo(TimeInterval? other)
    {
        return other is { }
            ? From.CompareTo(other.From)
            : 1;
    }

    public override bool Equals(object? obj) => Equals(obj as TimeInterval);

    public bool Equals(TimeInterval? other)
    {
        return !(other is null) && (From.Equals(other.From) && To.Equals(other.To));
    }

    public override int GetHashCode()
    {
        return From.GetHashCode();
    }

    public static bool operator ==(TimeInterval left, TimeInterval right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(TimeInterval left, TimeInterval right)
    {
        return !(left == right);
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
