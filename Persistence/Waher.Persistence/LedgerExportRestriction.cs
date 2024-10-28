using System;

namespace Waher.Persistence
{
	/// <summary>
	/// Contains basic ledger export restrictions.
	/// </summary>
	public class LedgerExportRestriction
	{
		/// <summary>
		/// Collections to export. If null, all collections will be exported.
		/// </summary>
		public string[] CollectionNames;

		/// <summary>
		/// Blocks to export. If null, all relevant blocks will be exported.
		/// </summary>
		public string[] BlockIds;

		/// <summary>
		/// Creators to export. If null, all relevant creators will be exported.
		/// </summary>
		public string[] Creators;

		/// <summary>
		/// Minimum value (if provided) of when a ledger block of information was created.
		/// </summary>
		public DateTime? MinCreated;

		/// <summary>
		/// If <see cref="MinCreated"/> is included
		/// </summary>
		public bool MinCreatedIncluded;

		/// <summary>
		/// Maximum value (if provided) of when a ledger block of information was created.
		/// </summary>
		public DateTime? MaxCreated;

		/// <summary>
		/// If <see cref="MaxCreated"/> is included
		/// </summary>
		public bool MaxCreatedIncluded;
	}
}
