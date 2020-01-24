using System;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for asynchronous enumerators.
	/// </summary>
	public interface IAsyncEnumerator
	{
		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		Task<bool> MoveNextAsync();
	}
}
