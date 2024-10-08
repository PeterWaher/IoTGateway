using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Synchronous enumerator
    /// </summary>
    public class SynchEnumerator : IResultSetEnumerator
    {
        private readonly IEnumerator e;

        /// <summary>
        /// Synchronous enumerator
        /// </summary>
        /// <param name="e">Underlying enumerator.</param>
        public SynchEnumerator(IEnumerator e)
        {
            this.e = e;
        }

        /// <summary>
        /// Current item.
        /// </summary>
        public object Current => e.Current;

        /// <summary>
        /// Tries to move to next item.
        /// </summary>
        /// <returns>If successful</returns>
        public bool MoveNext()
        {
            return e.MoveNext();
        }

        /// <summary>
        /// Tries to move to next item.
        /// </summary>
        /// <returns>If successful</returns>
        public Task<bool> MoveNextAsync()
        {
            return Task.FromResult(e.MoveNext());
        }

        /// <summary>
        /// Resets the enumerator
        /// </summary>
        public void Reset()
        {
            e.Reset();
        }
    }
}
