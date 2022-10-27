using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amg.Extensions;

/// <summary>
/// Extensions for IEnumerable
/// </summary>
public static class EnumerableExtensions
{
    private static readonly Serilog.ILogger Logger = Serilog.Log.Logger.ForContext(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

    /// <summary>
    /// Concat one (1) new element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <param name="newElement"></param>
    /// <returns></returns>
    public static IEnumerable<T> Concat<T>(this IEnumerable<T> e, T newElement)
    {
        return e.Concat(Enumerable.Repeat(newElement, 1));
    }

    /// <summary>
    /// Convert to strings and concatenate with separator
    /// </summary>
    /// <param name="e"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<object?> e, string separator)
    {
        return string.Join(separator, e.Where(_ => _ != null));
    }

    /// <summary>
    /// Convert to strings and concatenate with newline
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<object?> e)
    {
        return e.Join(System.Environment.NewLine);
    }

    /// <summary>
    /// Zips together two sequences. The shorter sequence is padded with default values.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipOrDefaultValue<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst?, TSecond?, TResult> resultSelector)
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement || secondHasElement)
                {
                    yield return resultSelector(
                        firstHasElement ? i0.Current : default,
                        secondHasElement ? i1.Current : default
                        );
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Zips together two sequences. The shorter sequence is padded with default values.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipPadSecond<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond?, TResult> resultSelector)
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement)
                {
                    yield return resultSelector(
                        i0.Current,
                        secondHasElement ? i1.Current : default
                        );
                }
                else
                {
                    break;
                }
            }
        }
    }

#nullable disable
    /// <summary>
    /// Zips together two sequences. The shorter sequence is padded with default values.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipOrDefault<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        where TResult : class
        where TFirst : class
        where TSecond : class
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement || secondHasElement)
                {
                    yield return resultSelector(
                        firstHasElement ? i0.Current : default(TFirst),
                        secondHasElement ? i1.Current : default(TSecond)
                        );
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipOr<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector,
        Func<TFirst, TResult> resultSelectorIfSecondElementIsMissing)
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement)
                {
                    yield return secondHasElement
                        ? resultSelector(i0.Current, i1.Current)
                        : resultSelectorIfSecondElementIsMissing(i0.Current);
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Pads the sequence with null if it is not long enough
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerable<T> Pad<T>(this IEnumerable<T> e, int count)
    {
        using (var currentItem = e.GetEnumerator())
        {
            for (int i = 0; i < count; ++i)
            {
                yield return currentItem.MoveNext()
                    ? currentItem.Current
                    : default(T);
            }
        }
    }
#nullable enable

    /// <summary>
    /// Returns the element i for which selector(i) is maximal
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Y"></typeparam>
    /// <param name="e"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static T? MaxElement<T, Y>(this IEnumerable<T> e, Func<T, Y> selector)
        where T : class
        where Y : IComparable
    {
        using (var i = e.GetEnumerator())
        {
#nullable disable
            T max = default(T);
            Y maxValue = default(Y);
            bool found = false;

            if (i.MoveNext())
            {
                max = i.Current;
                maxValue = selector(i.Current);
                found = true;
            }
            while (i.MoveNext())
            {
                var value = selector(i.Current);
                if (value.CompareTo(maxValue) == 1)
                {
                    maxValue = value;
                    max = i.Current;
                }
            }
            return found ? max : default(T);
#nullable enable
        }
    }

    /// <summary>
    /// Find an element by name. The name of an element i is determined by name(i). 
    /// </summary>
    /// Abbreviations are allowed: query can also be a substring of the name as long as it uniquely
    /// identifies an element.
    /// <typeparam name="T"></typeparam>
    /// <param name="candidates"></param>
    /// <param name="name">calculates the name of an element</param>
    /// <param name="query">the name (part) to be found.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">When query does not identify a named element.</exception>
    public static T? FindByNameOrDefault<T>(this IEnumerable<T> candidates, Func<T, string> name, string query) where T : class
    {
        var r = candidates.SingleOrDefault(option =>
            name(option).Equals(query, StringComparison.InvariantCultureIgnoreCase));

        if (r != null)
        {
            return r;
        }

        var matches = candidates.Where(option => query.IsAbbreviation(name(option)))
            .ToList();

        if (matches.Count > 1)
        {
            return null;
        }

        if (matches.Count == 1)
        {
            return matches[0];
        }

        return null;
    }

    /// <summary>
    /// Find an element by name. The name of an element i is determined by name(i). 
    /// </summary>
    /// Abbreviations are allowed: query can also be a substring of the name as long as it uniquely
    /// identifies an element.
    /// <typeparam name="T"></typeparam>
    /// <param name="candidates"></param>
    /// <param name="name">calculates the name of an element</param>
    /// <param name="query">the name (part) to be found.</param>
    /// <param name="itemsName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">When query does not identify a named element.</exception>
    public static T FindByName<T>(
        this IEnumerable<T> candidates,
        Func<T, string?> name,
        string query,
        string? itemsName = null)
    {
        var r = candidates.SingleOrDefault(option =>
            string.Equals(name(option), query, StringComparison.InvariantCultureIgnoreCase));

        if (r != null)
        {
            return r;
        }

        var matches = candidates.Where(option => query.IsAbbreviation(name(option)))
            .ToList();

        if (matches.Count > 1)
        {
            throw new ArgumentException($@"{query.Quote()} is ambiguous. Could be

{matches.Select(name).Join()}

");
        }

        if (matches.Count == 1)
        {
            return matches[0];
        }

        throw new ArgumentException($@"{query.Quote()} not found in {itemsName}

{candidates.Select(_ => "  " + name(_)).Join()}
");
    }

    /// <summary>
    /// Splits a string and returns the re-combined components
    /// </summary>
    /// "1.2.3".Lineage(".") returns ["1", "1.2", "1.2.3"]
    /// <param name="x"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static IEnumerable<string> Lineage(this string x, string separator)
    {
        var p = x.Split(new[] { separator }, StringSplitOptions.None);
        return p.Lineage().Select(_ => _.Join(separator));
    }

    /// <summary>
    /// Sequence of sequences that contain successively more elements of e
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Lineage<T>(this IEnumerable<T> e)
    {
        var count = 0;
        foreach (var i in e)
        {
            ++count;
            yield return e.Take(count);
        }
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> e) where T : class
    {
#pragma warning disable S1905 // Redundant casts should not be used
        // cast required from T? to T
        return e.Where(_ => _ != null).Cast<T>();
#pragma warning restore S1905 // Redundant casts should not be used
    }

    public static async Task<IEnumerable<T>> Result<T>(this IEnumerable<Task<T>> e)
    {
        var results = new List<T>();
        foreach (var i in e)
        {
            results.Add(await i);
        }
        return results;
    }

    /// <summary>
    /// Returns e except the last c elements
    /// </summary>
    /// <param name="e"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public static IEnumerable<T> TakeAllBut<T>(this IEnumerable<T> e, int c)
    {
        if (c == 0)
        {
            return e;
        }
        if (c < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(c), c, "cannot be negative");
        }
        return TakeAllButInternal(e, c);
    }

    static IEnumerable<T> TakeAllButInternal<T>(IEnumerable<T> e, int c)
    {
        var buffer = new T[c];
        int bi = 0;
        using (var i = e.GetEnumerator())
        {
            while (bi < c && i.MoveNext())
            {
                buffer[bi++] = i.Current;
            }
            while (i.MoveNext())
            {
                if (bi == c)
                {
                    bi = 0;
                }
                var o = buffer[bi];
                buffer[bi] = i.Current;
                ++bi;
                yield return o;
            }
        }
    }

    public static bool StartsWith<T>(this IEnumerable<T> e, IEnumerable<T> start)
    {
        return start.ZipPadSecond(e, (i1, i2) => object.Equals(i1, i2)).All(_ => _);
    }

    public static bool StartsWith<T>(this IEnumerable<T> e, IEnumerable<T> start, IEqualityComparer<T> equalityComparer)
    {
        return start.ZipPadSecond(e, (i1, i2) => equalityComparer.Equals(i1, i2)).All(_ => _);
    }

    public static int IndexOf<T>(this IEnumerable<T> e, T searchValue)
    {
        int index = 0;
        foreach (var i in e)
        {
            if (object.Equals(i, searchValue))
            {
                return index;
            }
            ++index;
        }
        return -1;
    }

    public static IEqualityComparer<T> CompareBy<T, Y>(Func<T?, Y?> map)
    {
        return new CompareByComparer<T, Y>(map);
    }

    class CompareByComparer<T, Y> : IEqualityComparer<T>
    {
        private readonly Func<T?, Y?> map;

        public CompareByComparer(Func<T?, Y?> map)
        {
            this.map = map;
        }

        public bool Equals(T? x, T? y)
            => object.Equals(map(x), map(y));

        public int GetHashCode(T obj) => map(obj)!.GetHashCode();
    }

    /// <summary>
    /// Zips together two sequences. The shorter sequence is padded with default values.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipPad<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst> padFirst,
        Func<TSecond> padSecond,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement || secondHasElement)
                {
                    yield return resultSelector(
                        firstHasElement ? i0.Current : padFirst(),
                        secondHasElement ? i1.Current : padSecond()
                        );
                }
                else
                {
                    break;
                }
            }
        }
    }

    public static IEnumerable<TResult> ZipPad<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TFirst?, TSecond?, TResult> resultSelector)
where TFirst : class
        where TSecond : class

    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement || secondHasElement)
                {
                    yield return resultSelector(
                        firstHasElement ? i0.Current : default(TFirst),
                        secondHasElement ? i1.Current : default(TSecond)
                        );
                }
                else
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Zips together two sequences. The second sequence is padded with default values.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="resultSelector"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> ZipPad<TFirst, TSecond, TResult>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second,
        Func<TSecond> padSecond,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        using (var i0 = first.GetEnumerator())
        using (var i1 = second.GetEnumerator())
        {
            while (true)
            {
                var firstHasElement = i0.MoveNext();
                var secondHasElement = i1.MoveNext();
                if (firstHasElement)
                {
                    yield return resultSelector(i0.Current, secondHasElement
                        ? i1.Current
                        : padSecond());
                }
                else
                {
                    break;
                }
            }
        }
    }

    public static IEnumerable<Y> SafeSelect<X, Y>(this IEnumerable<X> e, Func<X, Y> f)
    {
        foreach (var x in e)
        {
            Y y;
            try
            {
                y = f(x);
            }
            catch
            {
                continue;
            }
            yield return y;
        }
    }

    public async static Task<IList<Y>> SelectParallel<X, Y>(this IEnumerable<X> x, Func<X, Task<Y>> f, int batchSize = 16)
    {
        var batches = x.Batch(batchSize).Select(_ => _.Select(f));

        var results = new List<Y>();

        foreach (var batch in batches)
        {
            results.AddRange(await Task.WhenAll(batch));
        }

        return results;
    }

    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
                  this IEnumerable<TSource> source, int size)
    {
        TSource[]? bucket = null;
        var count = 0;

        foreach (var item in source)
        {
            if (bucket is null)
            {
                bucket = new TSource[size];
            }

            bucket[count++] = item;
            if (count != size)
                continue;

            yield return bucket;

            bucket = null;
            count = 0;
        }

        if (bucket != null && count > 0)
            yield return bucket.Take(count).ToArray();
    }

    public static IEnumerable<T> Top80<T>(this IEnumerable<T> e, Func<T, double> scalar) => e.Top(scalar, 0.8);

    public static IEnumerable<T> Top<T>(this IEnumerable<T> e, Func<T, double> scalar, double topPercentile)
    {
        e = e.ToList();
        var total = e.Sum(scalar);
        var sum = 0.0;
        var maxSum = total * topPercentile;
        foreach (var i in e.OrderByDescending(scalar))
        {
            sum += scalar(i);
            if (sum >= maxSum) break;
            yield return i;
        }
    }

    public static PivotTable<D, A0, A1> Pivot<T, D, A0, A1>(
        this IEnumerable<T> e,
        Func<T, A0> axis0,
        Func<T, A1> axis1,
        Func<IEnumerable<T>, D> data)
    {
        e = e.ToList();

        var dictionary = e.GroupBy(_ => Tuple.Create(axis0(_), axis1(_)))
            .ToDictionary(_ => _.Key, _ => data(_));

        var keys0 = dictionary.Keys.Select(_ => _.Item1).Distinct().OrderBy(_ => _).ToArray();
        var keys1 = dictionary.Keys.Select(_ => _.Item2).Distinct().OrderBy(_ => _).ToArray();
        var cells = new D[keys0.Length, keys1.Length];
        foreach (var i0 in Enumerable.Range(0, keys0.Length))
        {
            foreach (var i1 in Enumerable.Range(0, keys1.Length))
            {
                if (dictionary.TryGetValue(Tuple.Create(keys0[i0], keys1[i1]), out var v))
                {
                    cells[i0, i1] = v;
                }
            }
        }
        return new PivotTable<D, A0, A1>(keys0, keys1, cells);
    }
}

public record PivotTable<D, A0, A1>
{
    public PivotTable(A0[] axis0, A1[] axis1, D[,] data)
    {
        Axis0 = axis0;
        Axis1 = axis1;
        Data = data;
    }

    public A0[] Axis0 { get; private set; }
    public A1[] Axis1 { get; private set; }
    public D[,] Data { get; private set; }

    public IEnumerable<D> GetRow(int axis1)
    {
        foreach (var j in Enumerable.Range(0, Axis0.Length))
        {
            yield return Data[j, axis1];
        }
    }
}
