using System.Collections.Generic;

namespace Waher.Persistence
{
    /// <summary>
    /// Interface for paginated results.
    /// </summary>
    /// <typeparam name="T">Type of objects on the page.</typeparam>
    public interface IPage<T>
        where T : class
    {
        /// <summary>
        /// Items available in the page. The enumeration may be empty.
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        /// If there may be more pages following this page.
        /// </summary>
        bool More { get; }
    }
}
