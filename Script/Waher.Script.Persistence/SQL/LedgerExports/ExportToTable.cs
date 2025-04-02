using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Export is serialized into table form.
	/// </summary>
	public class ExportToTable : ILedgerExport
	{
		private readonly ChunkedList<IElement> elements = new ChunkedList<IElement>();
		private readonly ScriptNode[] columns;
		private readonly int width;
		private int height;
		private Variables variables;

		/// <summary>
		/// Export is serialized into object form.
		/// </summary>
		/// <param name="Columns">Column definitions.</param>
		public ExportToTable(ScriptNode[] Columns)
		{
			this.columns = Columns;
			this.width = this.columns.Length;
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
			string[] Headers = new string[this.width];
			int i;

			for (i = 0; i < this.width; i++)
			{
				if (this.columns[i] is VariableReference Ref)
					Headers[i] = Ref.VariableName;
				else
					Headers[i] = "c" + (i + 1).ToString();
			}

			return new ObjectMatrix(this.height, this.width, this.elements)
			{
				ColumnNames = Headers
			};
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartLedger()
		{
			this.height = 0;
			this.elements.Clear();

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
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndEntry()
		{
			if (this.variables is null)
				return false;

			foreach (ScriptNode Ref in this.columns)
			{
				try
				{
					IElement E;

					if (Ref.IsAsynchronous)
						E = await Ref.EvaluateAsync(this.variables);
					else
						E = Ref.Evaluate(this.variables);

					this.elements.Add(E);
				}
				catch (Exception ex)
				{
					this.elements.Add(new ObjectValue(ex));
				}
			}

			this.height++;

			return true;
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
