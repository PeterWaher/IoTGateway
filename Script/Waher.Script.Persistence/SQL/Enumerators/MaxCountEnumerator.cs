using System;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Enumerator that limits the return set to a maximum number of records.
    /// </summary>
    public class MaxCountEnumerator : IResultSetEnumerator
    {
        private readonly IResultSetEnumerator e;
        private readonly int count0;
        private int count;

        /// <summary>
        /// Enumerator that limits the return set to a maximum number of records.
        /// </summary>
        /// <param name="ItemEnumerator">Item enumerator</param>
        /// <param name="Count">Maximum number of records to enumerate.</param>
        public MaxCountEnumerator(IResultSetEnumerator ItemEnumerator, int Count)
        {
            e = ItemEnumerator;
            count = count0 = Count;
        }

        /// <summary>
        /// <see cref="IEnumerator.Current"/>
        /// </summary>
        public object Current => e.Current;

        /// <summary>
        /// <see cref="IEnumerator.MoveNext"/>
        /// </summary>
        public bool MoveNext()
        {
            return MoveNextAsync().Result;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if
        /// the enumerator has passed the end of the collection.</returns>
        /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
        public async Task<bool> MoveNextAsync()
        {
            if (count <= 0 || !await e.MoveNextAsync())
                return false;

            count--;

            return true;
        }

        /// <summary>
        /// <see cref="IEnumerator.Reset"/>
        /// </summary>
        public void Reset()
        {
            count = count0;
            e.Reset();
        }
    }
}
