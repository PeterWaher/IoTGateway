using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// </summary>
	public class ExportColumnFilter : ILedgerExport
	{
		private readonly ILedgerExport output;
		private readonly Dictionary<string, int> columnIndices;

		/// <summary>
		/// Class that filters properties to include only selected properties.
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		/// <param name="ColumnIndices">Columns to include in export.</param>
		public ExportColumnFilter(ILedgerExport Output, Dictionary<string, int> ColumnIndices)
		{
			this.output = Output;
			this.columnIndices = ColumnIndices;
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartLedger()
		{
			return this.output?.StartLedger() ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndLedger()
		{
			return this.output?.EndLedger() ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartCollection(string CollectionName)
		{
			return this.output?.StartCollection(CollectionName) ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndCollection()
		{
			return this.output?.EndCollection() ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartBlock(string BlockID)
		{
			return this.output?.StartBlock(BlockID) ?? Task.FromResult(true);
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> BlockMetaData(string Key, object Value)
		{
			if (this.columnIndices.ContainsKey(Key) && !(this.output is null))
				return this.output.BlockMetaData(Key, Value);
			else
				return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndBlock()
		{
			return this.output?.EndBlock() ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			return this.output?.StartEntry(ObjectId, TypeName, EntryType, EntryTimestamp) ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.columnIndices.ContainsKey(PropertyName) && !(this.output is null))
				return this.output.ReportProperty(PropertyName, PropertyValue);
			else
				return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndEntry()
		{
			return this.output?.EndEntry() ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when the collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			return this.output?.CollectionCleared(EntryTimestamp) ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportError(string Message)
		{
			return this.output?.ReportError(Message) ?? Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportException(Exception Exception)
		{
			return this.output?.ReportException(Exception) ?? Task.FromResult(true);
		}
	}
}
