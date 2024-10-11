using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Only exports entries matching a given condition (or conditions)
	/// </summary>
	public class ExportCondition : CustomEntryExport
	{
		private readonly LinkedList<KeyValuePair<string, object>> currentProperties = new LinkedList<KeyValuePair<string, object>>();
		private readonly List<KeyValuePair<string, ScriptNode>> additionalFields;
		private readonly ScriptNode condition;
		private readonly ObjectProperties properties = null;
		private readonly Variables contextVariables;
		private readonly Variables entryVariables;
		private readonly Variable variableObjectId;
		private readonly Variable variableTypeName;
		private readonly Variable variableEntryType;
		private readonly Variable variableTimestamp;
		private readonly Variable variableBlockId;
		private readonly Variable variableCollection;
		private string currentObjectId;
		private string currentTypeName;
		private EntryType currentEntryType;
		private DateTimeOffset currentEntryTimestamp;

		/// <summary>
		/// Only exports entries matching a given condition (or conditions)
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		/// <param name="Condition">Condition to allow export.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="AdditionalFields">Additional calculated fields.</param>
		public ExportCondition(ILedgerExport Output, ScriptNode Condition, Variables Variables,
			List<KeyValuePair<string, ScriptNode>> AdditionalFields)
			: base(Output)
		{
			this.condition = Condition;
			this.contextVariables = Variables;
			this.additionalFields = AdditionalFields;

			this.properties = new ObjectProperties(
				new GenericObject(string.Empty, string.Empty, Guid.Empty),
				this.contextVariables);

			this.entryVariables = new Variables()
			{
				ContextVariables = this.properties,
				ConsoleOut = Variables.ConsoleOut
			};

			this.variableObjectId = this.entryVariables.Add("ObjectId", this.currentObjectId);
			this.variableTypeName = this.entryVariables.Add("TypeName", this.currentTypeName);
			this.variableEntryType = this.entryVariables.Add("EntryType", this.currentEntryType);
			this.variableTimestamp = this.entryVariables.Add("Timestamp", this.currentEntryTimestamp);
			this.variableBlockId = this.entryVariables.Add("BlockId", this.StartedBlock);
			this.variableCollection = this.entryVariables.Add("Collection", this.StartedCollection);
		}

		/// <summary>
		/// Entry variables.
		/// </summary>
		public Variables EntryVariables => this.entryVariables;

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> StartCollection(string CollectionName)
		{
			this.variableCollection.SetValue(CollectionName);

			return base.StartCollection(CollectionName);
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> StartBlock(string BlockID)
		{
			this.variableBlockId.SetValue(BlockID);

			return base.StartBlock(BlockID);
		}

		/// <summary>
		/// Reports block meta-data.
		/// </summary>
		/// <param name="Key">Meta-data key.</param>
		/// <param name="Value">Meta-data value.</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> BlockMetaData(string Key, object Value)
		{
			this.entryVariables[Key] = Value;

			return base.BlockMetaData(Key, Value);
		}

		/// <summary>
		/// Is called when an entry is started.
		/// </summary>
		/// <param name="ObjectId">ID of object.</param>
		/// <param name="TypeName">Type name of object.</param>
		/// <param name="EntryType">Type of entry</param>
		/// <param name="EntryTimestamp">Timestamp of entry</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> StartEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			this.currentObjectId = ObjectId;
			this.currentTypeName = TypeName;
			this.currentEntryType = EntryType;
			this.currentEntryTimestamp = EntryTimestamp;

			this.variableObjectId.SetValue(this.currentObjectId);
			this.variableTypeName.SetValue(this.currentTypeName);
			this.variableEntryType.SetValue(this.currentEntryType);
			this.variableTimestamp.SetValue(this.currentEntryTimestamp);

			this.currentProperties.Clear();

			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when a property is reported.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <param name="PropertyValue">Property value.</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			this.currentProperties.AddLast(new KeyValuePair<string, object>(PropertyName, PropertyValue));
			return Task.FromResult(true);
		}

		/// <summary>
		/// Is called when an entry is finished.
		/// </summary>
		/// <returns>If export can continue.</returns>
		public override async Task<bool> EndEntry()
		{
			if (!Guid.TryParse(this.currentObjectId, out Guid ObjectId))
				ObjectId = Guid.Empty;

			this.properties.Object = new GenericObject(this.StartedCollection,
				this.currentTypeName, ObjectId, this.currentProperties);

			if (!(this.additionalFields is null))
			{
				foreach (KeyValuePair<string, ScriptNode> P in this.additionalFields)
				{
					IElement E;

					try
					{
						if (P.Value.IsAsynchronous)
							E = await P.Value.EvaluateAsync(this.entryVariables);
						else
							E = P.Value.Evaluate(this.entryVariables);
					}
					catch (ScriptReturnValueException ex)
					{
						E = ex.ReturnValue;
					}
					catch (Exception ex)
					{
						E = new ObjectValue(ex);
					}

					this.entryVariables[P.Key] = E;
					this.currentProperties.AddLast(new KeyValuePair<string, object>(P.Key, E.AssociatedObjectValue));
				}
			}

			if (!(this.condition is null))
			{
				try
				{
					IElement E;

					if (this.condition.IsAsynchronous)
						E = await this.condition.EvaluateAsync(this.entryVariables);
					else
						E = this.condition.Evaluate(this.entryVariables);

					if (!(E.AssociatedObjectValue is bool B && B))
						return true;
				}
				catch (Exception)
				{
					return true;
				}
			}

			if (!await base.StartEntry(this.currentObjectId, this.currentTypeName, this.currentEntryType, this.currentEntryTimestamp))
				return false;

			foreach (KeyValuePair<string, object> P in this.currentProperties)
			{
				if (!await base.ReportProperty(P.Key, P.Value))
					return false;
			}

			if (!await base.EndEntry())
				return false;

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
		public override bool IncludeEntry(string ObjectId, string TypeName, EntryType EntryType, DateTimeOffset EntryTimestamp)
		{
			return true;
		}

		/// <summary>
		/// If a non-entry event should be included.
		/// </summary>
		/// <returns>If non-entry event should be included</returns>
		public override bool IncludeNonEntryEvent()
		{
			return true;
		}
	}
}
