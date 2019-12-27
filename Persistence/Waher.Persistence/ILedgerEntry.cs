using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence
{
	/// <summary>
	/// Ledger entry type.
	/// </summary>
	public enum EntryType
	{
		/// <summary>
		/// New entry
		/// </summary>
		New,

		/// <summary>
		/// Update entry
		/// </summary>
		Update,

		/// <summary>
		/// Delete entry
		/// </summary>
		Delete,

		/// <summary>
		/// Collection cleared
		/// </summary>
		Clear
	}

	/// <summary>
	/// Interface for ledger entries.
	/// </summary>
	/// <typeparam name="T">Type of objects being processed.</typeparam>
	public interface ILedgerEntry<T>
	{
		/// <summary>
		/// Type of ledger entry
		/// </summary>
		EntryType EntryType
		{
			get;
		}

		/// <summary>
		/// Timestamp of entry
		/// </summary>
		DateTimeOffset EntryTimestamp
		{
			get;
		}

		/// <summary>
		/// Entry
		/// </summary>
		T Object
		{
			get;
		}
	}
}
