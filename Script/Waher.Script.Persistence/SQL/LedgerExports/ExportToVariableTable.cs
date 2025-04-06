using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Export is serialized into table form, with an undefined number of columns.
	/// </summary>
	public class ExportToVariableTable : ILedgerExport
	{
		private readonly ChunkedList<KeyValuePair<string, IElement>> collectionProperties = new ChunkedList<KeyValuePair<string, IElement>>();
		private readonly ChunkedList<KeyValuePair<string, IElement>> blockProperties = new ChunkedList<KeyValuePair<string, IElement>>();
		private readonly ChunkedList<KeyValuePair<string, IElement>> entryProperties = new ChunkedList<KeyValuePair<string, IElement>>();
		private readonly ChunkedList<IElement[]> rows = new ChunkedList<IElement[]>();
		private readonly ChunkedList<IElement> currentRow = new ChunkedList<IElement>();
		private readonly ChunkedList<string> headers = new ChunkedList<string>();
		private readonly Dictionary<string, int> headerOrder = new Dictionary<string, int>();
		private int width;
		private int height;
		private Variables variables;

		/// <summary>
		/// Export is serialized into table form, with an undefined number of columns.
		/// </summary>
		public ExportToVariableTable()
		{
		}

		/// <summary>
		/// Variables used to compute calculated column values.
		/// </summary>
		public Variables Variables
		{
			get => this.variables;
			internal set => this.variables = value;
		}

		/// <summary>
		/// Converts the exported information into an object matrix with column headers.
		/// </summary>
		/// <returns>Object matrix.</returns>
		public ObjectMatrix ToMatrix()
		{
			ChunkedList<IElement> Elements = new ChunkedList<IElement>();

			foreach (IElement[] Row in this.rows)
			{
				foreach (IElement E in Row)
					Elements.Add(E ?? ObjectValue.Null);

				int Padding = this.width - Row.Length;

				while (Padding > 0)
				{
					Elements.Add(ObjectValue.Null);
					Padding--;
				}
			}

			return new ObjectMatrix(this.height, this.width, Elements)
			{
				ColumnNames = this.headers.ToArray()
			};
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartLedger()
		{
			this.height = 0;
			this.width = 0;

			this.rows.Clear();
			this.headerOrder.Clear();
			this.headers.Clear();

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when export of ledger is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndLedger()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartCollection(string CollectionName)
		{
			this.collectionProperties.Clear();
			this.collectionProperties.Add(new KeyValuePair<string, IElement>("Collection", new StringValue(CollectionName)));

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndCollection()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartBlock(string BlockID)
		{
			this.blockProperties.Clear();
			this.blockProperties.Add(new KeyValuePair<string, IElement>("BlockId", new StringValue(BlockID)));

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
			this.blockProperties.Add(new KeyValuePair<string, IElement>(Key, Expression.Encapsulate(Value)));
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a block in a collection is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndBlock()
		{
			return Task.FromResult(true);
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
			this.entryProperties.Clear();
			this.entryProperties.Add(new KeyValuePair<string, IElement>("ObjectId", new StringValue(ObjectId)));
			this.entryProperties.Add(new KeyValuePair<string, IElement>("TypeName", new StringValue(TypeName)));
			this.entryProperties.Add(new KeyValuePair<string, IElement>("EntryType", new ObjectValue(EntryType)));
			this.entryProperties.Add(new KeyValuePair<string, IElement>("Timestamp", new ObjectValue(EntryTimestamp)));

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			this.entryProperties.Add(new KeyValuePair<string, IElement>(PropertyName, Expression.Encapsulate(PropertyValue)));
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> EndEntry()
		{
			if (this.variables is null)
				return Task.FromResult(false);

			this.currentRow.Clear();

			foreach (KeyValuePair<string, IElement> P in this.collectionProperties)
				this.AddPropertyToRow(P.Key, P.Value);

			foreach (KeyValuePair<string, IElement> P in this.blockProperties)
				this.AddPropertyToRow(P.Key, P.Value);

			foreach (KeyValuePair<string, IElement> P in this.entryProperties)
				this.AddPropertyToRow(P.Key, P.Value);

			this.rows.Add(this.currentRow.ToArray());
			this.height++;

			int c = this.currentRow.Count;
			if (c > this.width)
				this.width = c;

			return Task.FromResult(true);
		}

		private void AddPropertyToRow(string Name, IElement Value)
		{
			if (!this.headerOrder.TryGetValue(Name, out int i))
			{
				i = this.headers.Count;
				this.headerOrder[Name] = i;
				this.headers.Add(Name);
			}

			int c = this.currentRow.Count;

			if (c <= i)
			{
				while (c < i - 1)
				{
					this.currentRow.Add(null);
					c++;
				}

				this.currentRow.Add(Value);
			}
			else
				this.currentRow[i] = Value;
		}

		/// <summary>
		/// Is called when the collection has been cleared.
		/// </summary>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> CollectionCleared(DateTimeOffset EntryTimestamp)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an error is reported.
		/// </summary>
		/// <param name="Message">Error message.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportError(string Message)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>If export can continue.</returns>
		public Task<bool> ReportException(Exception Exception)
		{
			return Task.FromResult(true);
		}
	}
}
