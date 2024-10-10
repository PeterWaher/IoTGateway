using System.Collections;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for objects with asynchronous enumerators.
	/// </summary>
	public interface IAsyncEnumerable : IEnumerable
	{
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An System.Collections.IEnumerator object that can be used to iterate through
		/// the collection.
		/// </returns>
		IAsyncEnumerator GetAsyncEnumerator();
	}

	/// <summary>
	/// Interface for objects with asynchronous enumerators.
	/// </summary>
	public interface IAsyncEnumerable<T> : System.Collections.Generic.IEnumerable<T>, IAsyncEnumerable
	{
	}
}
