namespace Amg.Extensions;

public static class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<T> NotNull<T>(this IAsyncEnumerable<T?> asyncEnumerable)
    {
        return asyncEnumerable.Where(x => x is not null).Select(_ => _!);
    }
}
