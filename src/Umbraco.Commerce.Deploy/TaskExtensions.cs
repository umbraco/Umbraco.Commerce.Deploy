using System.Collections.Generic;
using System.Threading.Tasks;

namespace Umbraco.Commerce.Deploy;

internal static class TaskExtensions
{
    internal static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this Task<IEnumerable<T>> task)
    {
        IEnumerable<T> results = await task;
        foreach(T item in results)
        {
            yield return item;
        }
    }
}
