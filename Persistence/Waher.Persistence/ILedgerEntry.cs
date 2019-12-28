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
		New = 1,

		/// <summary>
		/// Update entry
		/// </summary>
		Update = 2,

		/// <summary>
		/// Delete entry
		/// </summary>
		Delete = 3,

		/// <summary>
		/// Collection cleared
		/// </summary>
		Clear = 4
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
