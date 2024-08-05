using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Commerce.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        var results = new List<T>();

        await foreach (T item in items.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            results.Add(item);
        }

        return results;
    }
}
