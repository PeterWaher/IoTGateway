using System;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Enumerator that skips a given number of result records.
    /// </summary>
    public class OffsetEnumerator : IResultSetEnumerator
    {
        private readonly IResultSetEnumerator e;
        private readonly int offset0;
        private int offset;

        /// <summary>
        /// Enumerator that skips a given number of result records.
        /// </summary>
        /// <param name="ItemEnumerator">Item enumerator</param>
        /// <param name="Offset">Number of records to skip.</param>
        public OffsetEnumerator(IResultSetEnumerator ItemEnumerator, int Offset)
        {
            e = ItemEnumerator;
            offset = offset0 = Offset;
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
            while (await e.MoveNextAsync())
            {
                if (offset > 0)
                    offset--;
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// <see cref="IEnumerator.Reset"/>
        /// </summary>
        public void Reset()
        {
            offset = offset0;
            e.Reset();
        }
    }
}
