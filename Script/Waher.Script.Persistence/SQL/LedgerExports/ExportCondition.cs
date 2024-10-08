using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.LedgerExports
{
	/// <summary>
	/// Only exports entries matching a given condition (or conditions)
	/// </summary>
	public class ExportCondition : CustomEntryExport
	{
		private readonly LinkedList<KeyValuePair<string, object>> currentProperties = new LinkedList<KeyValuePair<string, object>>();
		private readonly ScriptNode condition;
		private readonly Variables contextVariables;
		private Variables entryVariables;
		private ObjectProperties properties = null;
		private string currentObjectId;
		private string currentTypeName;
		private EntryType currentEntryType;
		private DateTimeOffset currentEntryTimestamp;
		private Variable variableObjectId;
		private Variable variableTypeName;
		private Variable variableEntryType;
		private Variable variableTimestamp;
		private Variable variableBlockId;
		private Variable variableCollection;

		/// <summary>
		/// Only exports entries matching a given condition (or conditions)
		/// </summary>
		/// <param name="Output">Underlying output.</param>
		/// <param name="Condition">Condition to allow export.</param>
		/// <param name="Variables">Current set of variables.</param>
		public ExportCondition(ILedgerExport Output, ScriptNode Condition, Variables Variables)
			: base(Output)
		{
			this.condition = Condition;
			this.contextVariables = Variables;
			this.properties = null;
			this.entryVariables = null;
		}

		/// <summary>
		/// Is called when a collection is started.
		/// </summary>
		/// <param name="CollectionName">Name of collection</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> StartCollection(string CollectionName)
		{
			this.variableCollection?.SetValue(CollectionName);

			return base.StartCollection(CollectionName);
		}

		/// <summary>
		/// Is called when a block in a collection is started.
		/// </summary>
		/// <param name="BlockID">Block ID</param>
		/// <returns>If export can continue.</returns>
		public override Task<bool> StartBlock(string BlockID)
		{
			this.variableBlockId?.SetValue(BlockID);

			return base.StartBlock(BlockID);
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

			this.currentProperties.Clear();

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

			GenericObject Obj = new GenericObject(this.StartedCollection, this.currentTypeName, ObjectId, this.currentProperties);
			bool UpdateEntryVariables = true;

			if (this.properties is null)
			{
				this.properties = new ObjectProperties(Obj, this.contextVariables);

				if (this.entryVariables is null)
				{
					this.variableObjectId = new Variable("ObjectId", this.currentObjectId);
					this.variableTypeName = new Variable("TypeName", this.currentTypeName);
					this.variableEntryType = new Variable("EntryType", this.currentEntryType);
					this.variableTimestamp = new Variable("Timestamp", this.currentEntryTimestamp);
					this.variableBlockId = new Variable("BlockId", this.StartedBlock);
					this.variableCollection = new Variable("Collection", this.StartedCollection);

					this.entryVariables = new Variables()
					{
						ContextVariables = this.properties
					};

					UpdateEntryVariables = false;
				}
			}
			else
				this.properties.Object = Obj;

			if (UpdateEntryVariables)
			{
				this.variableObjectId.SetValue(this.currentObjectId);
				this.variableTypeName.SetValue(this.currentTypeName);
				this.variableEntryType.SetValue(this.currentEntryType);
				this.variableTimestamp.SetValue(this.currentEntryTimestamp);
			}

			bool ExportEntry;

			try
			{
				IElement E;

				if (this.condition.IsAsynchronous)
					E = await this.condition.EvaluateAsync(this.entryVariables);
				else
					E = this.condition.Evaluate(this.entryVariables);

				ExportEntry = E.AssociatedObjectValue is bool B && B;
			}
			catch (Exception)
			{
				ExportEntry = false;
			}

			if (ExportEntry)
			{
				if (!await base.StartEntry(this.currentObjectId, this.currentTypeName, this.currentEntryType, this.currentEntryTimestamp))
					return false;

				foreach (KeyValuePair<string, object> P in this.currentProperties)
				{
					if (!await base.ReportProperty(P.Key, P.Value))
						return false;
				}

				if (!await base.EndEntry())
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
		public override Task<bool> ReportProperty(string PropertyName, object PropertyValue)
		{
			this.currentProperties.AddLast(new KeyValuePair<string, object>(PropertyName, PropertyValue));
			return Task.FromResult(true);
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
