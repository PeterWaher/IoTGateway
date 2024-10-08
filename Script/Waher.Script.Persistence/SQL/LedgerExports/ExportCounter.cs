using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Class that counts exported elements.
	/// </summary>
	public class ExportCounter : ILedgerExport
	{
		private readonly ILedgerExport output;
		private long nrCollections = 0;
		private long nrBlocks = 0;
		private long nrEntries = 0;
		private long nrProperties = 0;

		/// <summary>
		/// Class that counts exported elements.
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		public ExportCounter(ILedgerExport Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Number of collections processed
		/// </summary>
		public long NrCollections => this.nrCollections;
		
		/// <summary>
		/// Number of blocks processed
		/// </summary>
		public long NrBlocks => this.nrBlocks;
		
		/// <summary>
		/// Number of entries processed
		/// </summary>
		public long NrEntries => this.nrEntries;
		
		/// <summary>
		/// Number of properties processed
		/// </summary>
		public long NrProperties => this.nrProperties;

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartLedger()
		{
			return this.output.StartLedger();
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndLedger()
		{
			return this.output.EndLedger();
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartCollection(string CollectionName)
		{
			this.nrCollections++;
			return this.output.StartCollection(CollectionName);
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndCollection()
		{
			return this.output.EndCollection();
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartBlock(string BlockID)
		{
			this.nrBlocks++;
			return this.output.StartBlock(BlockID);
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> BlockMetaData(string Key, object Value)
		{
			return this.output.BlockMetaData(Key, Value);
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndBlock()
		{
			return this.output.EndBlock();
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
			this.nrEntries++;
			return this.output.StartEntry(ObjectId, TypeName, EntryType, EntryTimestamp);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndEntry()
		{
			return this.output.EndEntry();
		}

		/// <summary>
		/// Is called when the collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			return this.output.CollectionCleared(EntryTimestamp);
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			this.nrProperties++;
			return this.output.ReportProperty(PropertyName, PropertyValue);
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportError(string Message)
		{
			return this.output.ReportError(Message);
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportException(Exception Exception)
		{
			return this.output.ReportException(Exception);
		}
	}
}
