using System;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Limits the number of exported entries
	/// </summary>
	public class ExportEntryMaxCount : CustomEntryExport
	{
		private int maxCount;

		/// <summary>
		/// Limits the number of exported entries
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		/// <param name="MaxCount">Maximum number of entries to return.</param>
		public ExportEntryMaxCount(ILedgerExport Output, int MaxCount)
			: base(Output)
		{
			this.maxCount = MaxCount;
		}

		/// <summary>
		/// If an entry should be included.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If entry should be included</returns>
		public override bool IncludeEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			if (this.maxCount > 0)
			{
				this.maxCount--;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// If a non-entry event should be included.
		/// </summary>
		/// <returns>If non-entry event should be included</returns>
		public override bool IncludeNonEntryEvent()
		{
			return this.maxCount >= 0;
		}

		/// <summary>
		/// If export should be continued or not.
		/// </summary>
		/// <returns>true to continue export, false to terminate export.</returns>
		public override bool ContinueExport() => this.maxCount > 0;
	}
}
