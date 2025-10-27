using System.Collections.Generic;
using System.Linq;

namespace An2WinFileTransfer.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether an enumerable is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="source">The enumerable to check.</param>
        /// <returns>True if the enumerable is null or empty; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source is null || !source.Any();
        }
    }
}
