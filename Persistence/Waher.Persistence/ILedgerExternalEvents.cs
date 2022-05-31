namespace Waher.Persistence
{
	/// <summary>
	/// Interface for proxy for reporting changes to the ledger from external sources.
	/// </summary>
	public interface ILedgerExternalEvents
	{
		/// <summary>
		/// Raise <see cref="Ledger.EntryAdded"/> event.
		/// </summary>
		/// <param name="Object">Object reference</param>
		void RaiseEntryAdded(object Object);

		/// <summary>
		/// Raise <see cref="Ledger.EntryUpdated"/> event.
		/// </summary>
		/// <param name="Object">Object reference</param>
		void RaiseEntryUpdated(object Object);

		/// <summary>
		/// Raise <see cref="Ledger.EntryDeleted"/> event.
		/// </summary>
		/// <param name="Object">Object reference</param>
		void RaiseEntryDeleted(object Object);

		/// <summary>
		/// Raise <see cref="Ledger.CollectionCleared"/> event.
		/// </summary>
		/// <param name="Collection">Collection name.</param>
		void RaiseCollectionCleared(string Collection);
	}
}
