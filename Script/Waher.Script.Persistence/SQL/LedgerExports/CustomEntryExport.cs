using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Abstract base class for ledger exports with custom entry filter rules.
	/// </summary>
	public abstract class CustomEntryExport : ILedgerExport
	{
		private readonly Dictionary<string, object> blockMetaData = new Dictionary<string, object>();
		private readonly ILedgerExport output;
		private string startedCollection = null;
		private string startedBlock = null;
		private bool startedCollectionExported = false;
		private bool startedBlockExported = false;
		private bool hasBlockMetaData = false;
		private bool includeCurrentEntry = false;

		/// <summary>
		/// Abstract base class for ledger exports with custom entry filter rules.
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		public CustomEntryExport(ILedgerExport Output)
		{
			this.output = Output;
		}

		/// <summary>
		/// Collection that is currently being processed.
		/// </summary>
		protected string StartedCollection => this.startedCollection;

		/// <summary>
		/// Block that is currently being processed.
		/// </summary>
		protected string StartedBlock => this.startedBlock;

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
		public virtual Task<bool> StartCollection(string CollectionName)
		{
			this.startedCollection = CollectionName;
			this.startedCollectionExported = false;

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndCollection()
		{
			if (this.startedCollectionExported)
			{
				if (!await this.output.EndCollection())
					return false;

				this.startedCollectionExported = false;
			}

			this.startedCollection = null;

			return true;
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public virtual Task<bool> StartBlock(string BlockID)
		{
			this.startedBlock = BlockID;
			this.startedBlockExported = false;
			this.hasBlockMetaData = false;

			return Task.FromResult(true);
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> BlockMetaData(string Key, object Value)
		{
			this.blockMetaData[Key] = Value;
			this.hasBlockMetaData = true;

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndBlock()
		{
			if (this.startedBlockExported)
			{
				if (!await this.output.EndBlock())
					return false;

				this.startedBlockExported = false;
			}

			if (this.hasBlockMetaData)
			{
				this.blockMetaData.Clear();
				this.hasBlockMetaData = false;
			}

			this.startedBlock = null;

			return true;
		}

		/// <summary>
		/// If an entry should be included.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If entry should be included</returns>
		public abstract bool IncludeEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp);

		/// <summary>
		/// If a non-entry event should be included.
		/// </summary>
		/// <returns>If non-entry event should be included</returns>
		public abstract bool IncludeNonEntryEvent();

		/// <summary>
		/// If export should be continued or not.
		/// </summary>
		/// <returns>true to continue export, false to terminate export.</returns>
		public virtual bool ContinueExport() => true;

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public virtual async Task<bool> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			if (this.IncludeEntry(ObjectId, TypeName, EntryType, EntryTimestamp))
			{
				if (!await this.OutputPendingInfo())
					return false;

				this.includeCurrentEntry = true;

				return await this.output.StartEntry(ObjectId, TypeName, EntryType, EntryTimestamp);
			}
			else
			{
				this.includeCurrentEntry = false;
				return true;
			}
		}

		private async Task<bool> OutputPendingInfo()
		{
			if (!this.startedCollectionExported && !(this.startedCollection is null))
			{
				if (!await this.output.StartCollection(this.startedCollection))
					return false;

				this.startedCollectionExported = true;
			}

			if (!this.startedBlockExported && !(this.startedBlock is null))
			{
				if (!await this.output.StartBlock(this.startedBlock))
					return false;

				this.startedBlockExported = true;

				if (this.hasBlockMetaData)
				{
					foreach (KeyValuePair<string, object> P in this.blockMetaData)
					{
						if (!await this.output.BlockMetaData(P.Key, P.Value))
							return false;
					}

					this.blockMetaData.Clear();
					this.hasBlockMetaData = false;
				}
			}

			return true;
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public virtual async Task<bool> EndEntry()
		{
			if (this.includeCurrentEntry)
			{
				this.includeCurrentEntry = false;
				if (!await this.output.EndEntry())
					return false;
			}
			
			return this.ContinueExport();
		}

		/// <summary>
		/// Is called when the collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public async Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			if (this.IncludeNonEntryEvent())
			{
				if (!await this.OutputPendingInfo())
					return false;

				if (!await this.output.CollectionCleared(EntryTimestamp))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public virtual Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			if (this.includeCurrentEntry)
				return this.output.ReportProperty(PropertyName, PropertyValue);
			else
				return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportError(string Message)
		{
			if (this.IncludeNonEntryEvent())
				return this.output.ReportError(Message);
			else
				return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportException(Exception Exception)
		{
			if (this.IncludeNonEntryEvent())
				return this.output.ReportException(Exception);
			else
				return Task.FromResult(true);
		}
	}
}
