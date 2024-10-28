using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Export is serialized into calls to a lambda expression having one argument.
	/// </summary>
	public class ExportToLambda : ILedgerExport
	{
		private readonly ILambdaExpression lambda;
		private readonly IElement[] arguments = new IElement[] { ObjectValue.Null };
		private Dictionary<string, IElement> currentCollection = null;
		private Dictionary<string, IElement> currentBlock = null;
		private Dictionary<string, IElement> currentEntry = null;
		private Dictionary<string, IElement> propertiesInEvent = null;
		private Variables variables;

		/// <summary>
		/// Export is serialized into calls to a lambda expression having one argument.
		/// </summary>
		public ExportToLambda(ILambdaExpression Lambda)
		{
			this.lambda = Lambda;
		}

		/// <summary>
		/// Variables used to execute the lambda expression.
		/// </summary>
		public Variables Variables
		{
			get => this.variables;
			internal set => this.variables = value;
		}

		/// <summary>
		/// Is called when export of ledger is started.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public Task<bool> StartLedger()
		{
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
			this.currentCollection = new Dictionary<string, IElement>()
			{
				{ "Name", new StringValue(CollectionName) }
			};

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
			this.currentBlock = new Dictionary<string, IElement>()
			{
				{ "Id", new StringValue(BlockID) }
			};

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
			this.currentBlock[Key] = Expression.Encapsulate(Value);
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
			this.propertiesInEvent = new Dictionary<string, IElement>();
			this.currentEntry = new Dictionary<string, IElement>()
			{
				{ "ObjectId", new ObjectValue(ObjectId) },
				{ "TypeName", new ObjectValue(TypeName) },
				{ "EntryType", new ObjectValue(EntryType) },
				{ "Timestamp", new ObjectValue(EntryTimestamp) },
				{ "this", new ObjectValue(this.propertiesInEvent) }
			};

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
			this.propertiesInEvent[PropertyName] = Expression.Encapsulate(PropertyValue);
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public async Task<bool> EndEntry()
		{
			Dictionary<string, IElement> Data = new Dictionary<string, IElement>();

			foreach (KeyValuePair<string, IElement> P in this.currentCollection)
				Data[P.Key] = P.Value;

			foreach (KeyValuePair<string, IElement> P in this.currentBlock)
				Data[P.Key] = P.Value;

			foreach (KeyValuePair<string, IElement> P in this.currentEntry)
				Data[P.Key] = P.Value;

			foreach (KeyValuePair<string, IElement> P in this.propertiesInEvent)
				Data[P.Key] = P.Value;

			try
			{
				this.arguments[0] = new ObjectValue(Data);

				if (this.lambda.IsAsynchronous)
					await this.lambda.EvaluateAsync(this.arguments, this.variables);
				else
					this.lambda.Evaluate(this.arguments, this.variables);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
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
