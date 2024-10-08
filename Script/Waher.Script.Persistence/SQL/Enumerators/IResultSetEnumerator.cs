using System.Collections;
using Waher.Persistence;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Interface for result-set enumerators.
    /// </summary>
    public interface IResultSetEnumerator : IEnumerator, IAsyncEnumerator
    {
    }
}
