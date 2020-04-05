using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for ledger providers that can be plugged into the static <see cref="Ledger"/> class.
	/// </summary>
	public interface ILedgerProvider
	{
		/// <summary>
		/// Adds an entry to the ledger.
		/// </summary>
		/// <param name="Object">New object.</param>
		Task NewEntry(object Object);

		/// <summary>
		/// Updates an entry in the ledger.
		/// </summary>
		/// <param name="Object">Updated object.</param>
		Task UpdatedEntry(object Object);

		/// <summary>
		/// Deletes an entry in the ledger.
		/// </summary>
		/// <param name="Object">Deleted object.</param>
		Task DeletedEntry(object Object);

		/// <summary>
		/// Gets an eumerator for objects of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Type of object entries to enumerate.</typeparam>
		/// <returns>Enumerator object.</returns>
		Task<ILedgerEnumerator<T>> GetEnumerator<T>();

		/// <summary>
		/// Gets an eumerator for objects in a collection.
		/// </summary>
		/// <param name="CollectionName">Collection to enumerate.</param>
		/// <returns>Enumerator object.</returns>
		Task<ILedgerEnumerator<object>> GetEnumerator(string CollectionName);

		/// <summary>
		/// Called when processing starts.
		/// </summary>
		Task Start();

		/// <summary>
		/// Called when processing ends.
		/// </summary>
		Task Stop();

		/// <summary>
		/// Persists any pending changes.
		/// </summary>
		Task Flush();

		/// <summary>
		/// Gets an array of available collection.s
		/// </summary>
		/// <returns>Array of collections.</returns>
		Task<string[]> GetCollections();
	}
}
