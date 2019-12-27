using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Enumerator of ledger entries
	/// </summary>
	/// <typeparam name="T">Type of objects being processed.</typeparam>
	public interface ILedgerEnumerator<T> : IEnumerator<ILedgerEntry<T>>
	{
		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		/// The collection was modified after the enumerator was created.
		/// </exception>
		Task<bool> MoveNextAsync();
	}
}
