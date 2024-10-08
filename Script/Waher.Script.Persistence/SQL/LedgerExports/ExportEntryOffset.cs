using System;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Skips a number of entries
	/// </summary>
	public class ExportEntryOffset : CustomEntryExport
	{
		private int offset;

		/// <summary>
		/// Skips a number of entries
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		/// <param name="Offset">Number of entries to skip.</param>
		public ExportEntryOffset(ILedgerExport Output, int Offset)
			: base(Output)
		{
			this.offset = Offset;
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
			if (this.offset > 0)
			{
				this.offset--;
				return false;
			}
			else
				return true;
		}

		/// <summary>
		/// If a non-entry event should be included.
		/// </summary>
		/// <returns>If non-entry event should be included</returns>
		public override bool IncludeNonEntryEvent()
		{
			return this.offset <= 0;
		}

	}
}
